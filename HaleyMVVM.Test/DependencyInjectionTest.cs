using Haley.MVVM;
using Haley.Models;
using HaleyMVVM.Test.Models;
using Haley.Enums;
using Haley.Abstractions;
using System;
using Xunit;
using Xunit.Sdk;
using HaleyMVVM.Test.Interfaces;
using Microsoft.Xaml.Behaviors.Media;
using Haley.IOC;

namespace HaleyMVVM.Test
{
    public class DependencyInjectionTest
    {
        //IBaseContainer _diSingleton = ContainerStore.Singleton.DI;
        [Fact]
        public void Concrete__Equals()
        {
            //Arrange
            IBaseContainer _di = new MicroContainer();
            Person p_expected = new Person() { name = "Latha G" };
            _di.Register<Person>(p_expected);

            //Act
            var p_actual = _di.Resolve<Person>();

            //Assert
            Assert.Equal(p_expected, p_actual); //If not registered, this should be equal to what we send.
        }

        [Fact]
        public void ContainerSingleton_Equals()
        {
            //Arrange
            IBaseContainer _di = new MicroContainer();
            Person p_expected = new Person() { name = "Senguttuvan" };
            _di.Register<Person>(p_expected); //Register as containersingleton

            //Act
            var p_actual = _di.Resolve<Person>(ResolveMode.Transient); 

            //Assert
            Assert.Equal(p_expected, p_actual);
        }

        [Fact]
        public void WeakSingleton_NotEquals()
        {
            //Arrange
            IBaseContainer _di = new MicroContainer();
            Person p_expected = new Person() { name = "Senguttuvan" };
            _di.Register<Person>(p_expected,SingletonMode.ContainerWeakSingleton); //Register as containersingleton

            //Act
            var p_actual = _di.Resolve<Person>(ResolveMode.Transient); //Weak can be resolved.

            //Assert
            Assert.NotEqual(p_expected, p_actual);
        }

        [Fact]
        public void ForcedSingeltonCheck()
        {
            //Arrange
            IBaseContainer _di = new MicroContainer();
            string basename = "Pranav Krishna";
            Person p_expected = new Person() { name = basename };
            _di.Register<Person>(p_expected,SingletonMode.UniversalSingleton); //Registering as forced singleton. So, even if transient is requested, it should always give pranavkrishna

            //Act
            var p_actual = _di.Resolve<Person>(ResolveMode.Transient); //Since generating new instance, this should not be equal. However, we have registered person as ForcedSingleton. So whatever we do, we always get pranav krishna.

            //Assert
            Assert.Equal(p_expected, p_actual);
        }


        [Fact]
        public void ContainersSelfRegistrationCheck()
        {
            //Arrange
            //Set01
            IContainerFactory _factory = new MicroContainerFactory(new MicroContainer());

            //Set 02
            IBaseContainer _newbase = new MicroContainer(); //an isolated root.
            IControlContainer _newControl = new ControlContainer(_newbase);
            IWindowContainer _newWndw = new WindowContainer(_newbase);

            //var _houseFactory01 = ((IBaseContainer)_factory.DI).Resolve<HouseFactory>(); //This should have all relevance to main factory.

            var _houseFactory01 = ((IMicroContainerFactory)_factory).Services.Resolve<HouseFactory>(); //This should have all relevance to main factory.


            //Act
            var _house02 = _newbase.Resolve<House>(); //resolve using newbasecontainer
            var _oldhouse = ((IBaseContainer)_factory.Services)?.Resolve<House>(); //resolve using the main factory.

            //Assert
            Assert.Equal(_houseFactory01.house.Container.Id, _oldhouse.Container.Id); //Both houses should have received same base container.
            Assert.Equal(_newbase.Id, _house02.Container.Id); //New house has received new id.
            Assert.NotEqual(_houseFactory01.house.Container.Id, _house02.Container.Id); //Both houses resolved using different containers should have different ids.

        }

        [Fact]
        public void Concrete_CustomMapping()
        {
            //Arrange
            IBaseContainer _di = new MicroContainer();
            Person p1 = new Person() { name = "Senguttuvan" };
            _di.Register<Person>(p1);
            string expected = "BhadriNarayanan";

            //Act
            MappingProviderBase _mappingProvider = new MappingProviderBase();
            _mappingProvider.Add<Person>(null,new Person() { name = expected });
            var transient_actual = _di.ResolveTransient<Person>(_mappingProvider,MappingLevel.Current).name;
            var asregistered_actual = _di.Resolve<Person>(_mappingProvider).name;
            var asregistered_forced = _di.Resolve<Person>(_mappingProvider,currentOnlyAsTransient:true).name;
            //Assert
            Assert.Equal(expected, transient_actual);
            Assert.Equal(expected, asregistered_actual);
            Assert.Null(asregistered_forced); //Because, we force creation, so name will be null
        }

        [Fact]
        public void ConcreteMapping_Abstract()
        {
            //Arrange
            IBaseContainer _di = new MicroContainer();
            _di.ErrorHandling = ExceptionHandling.Throw;
            IPerson p1 = new SuperHero() { name = "Bruce Wayne", alter_ego="BatMan" };

            //Act
            Action act = () => _di.Register<IPerson>(p1);

            //Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void TypeMapping_Equals_Singleton()
        {
            //Arrange
            IBaseContainer _di = new MicroContainer();
            SuperHero p1 = new SuperHero() { name = "Bruce Wayne", alter_ego = "BatMan" };

            //Act
            _di.Register<IPerson,SuperHero>(p1);
            var _shero = _di.Resolve<IPerson>();

            //Assert
            Assert.Equal(p1,_shero);
        }

        [Fact]
        public void TypeMapping_NoConstructor_Instance()
        {
            //Arrange
            IBaseContainer _di = new MicroContainer();
            //Act
            _di.Register<IPerson, SuperHero>();
            var _shero = (SuperHero) _di.Resolve<IPerson>();

            //Assert
            Assert.NotNull(_shero);
        }

        [Fact]
        public void TypeMapping__IMapping_Resolve()
        {
            //Arrange
            IBaseContainer _di = new MicroContainer();
            string power = "Money";
            MappingProviderBase _mpb = new MappingProviderBase();
            _mpb.Add<string>(nameof(SuperHero.power), power, typeof(SuperHero), InjectionTarget.Property);
            //Act
            _di.Register<IPerson, SuperHero>(RegisterMode.ContainerWeakSingleton);
            var _shero = (SuperHero)_di.ResolveTransient<IPerson>(_mpb,MappingLevel.CurrentWithDependencies);
            
            //Assert
            Assert.Equal(power, _shero.power);
        }

        [Fact]
        public void TypeMapping__IMapping_Register()
        {
            //Arrange
            IBaseContainer _di = new MicroContainer();
            string power = "Money";
            MappingProviderBase _mpb = new MappingProviderBase();
            _mpb.Add<string>(nameof(SuperHero.power), power, typeof(SuperHero), InjectionTarget.Property);
            //Act
            _di.Register<IPerson, SuperHero>(_mpb,MappingLevel.Current);
            var _shero = (SuperHero)_di.Resolve<IPerson>();


            //Assert
            Assert.Equal(power, _shero.power);
        }

        [Fact]
        public void DifferentSingleton_TransientCheck()
        {
            //Arrange
            IBaseContainer _di = new MicroContainer();
            
            _di.RegisterWithKey<IPerson, SuperHero>("G0", RegisterMode.ContainerWeakSingleton); //Even within same container, will have the capability to be resolved as transient.
            _di.RegisterWithKey<IPerson, SuperHero>("G1"); //Will be resolved differently across containers.
            _di.RegisterWithKey<IPerson, SuperHero>("G2",RegisterMode.UniversalSingleton); //Will be resolved same across all child containers.

            //Act
            var g0Hero = (SuperHero)_di.Resolve<IPerson>("G0");
            var g1Hero = (SuperHero)_di.Resolve<IPerson>("G1");
            var g2Hero = (SuperHero)_di.Resolve<IPerson>("G2");

            g0Hero.IncreasePower(2); //One increase
            g1Hero.IncreasePower(2); 
            g2Hero.IncreasePower(3);

            //Assert
            Assert.Equal(g0Hero.value, g1Hero.value);
            Assert.NotEqual(g1Hero.value, g2Hero.value);

            //Act
            var g0HeroTrans = (SuperHero)_di.ResolveTransient<IPerson>("G0", TransientCreationLevel.Current); //This alone should be transient.
            var g1HeroTrans = (SuperHero)_di.ResolveTransient<IPerson>("G1", TransientCreationLevel.Current);
            var g2HeroTrans = (SuperHero)_di.ResolveTransient<IPerson>("G2", TransientCreationLevel.Current);

            //Assert
            Assert.NotEqual(g0Hero.value, g0HeroTrans.value); //Since g0herotrans is not a new instance with no power.
            Assert.Equal(g1Hero.value, g1HeroTrans.value); //Cannot create trans.
            Assert.Equal(g2Hero.value, g2HeroTrans.value);
        }

        [Fact]
        public void DifferentSingleton_ChildContainer()
        {
            //Arrange
            IBaseContainer _di = new MicroContainer();

            _di.RegisterWithKey<IPerson, SuperHero>("G0", RegisterMode.ContainerWeakSingleton); //Even within same container, will have the capability to be resolved as transient.
            _di.RegisterWithKey<IPerson, SuperHero>("G1"); //Will be resolved differently across containers.
            _di.RegisterWithKey<IPerson, SuperHero>("G2", RegisterMode.UniversalSingleton); //Will be resolved same across all child containers.

            //Act
            var g0Hero = (SuperHero)_di.Resolve<IPerson>("G0");
            var g1Hero = (SuperHero)_di.Resolve<IPerson>("G1");
            var g2Hero = (SuperHero)_di.Resolve<IPerson>("G2");

            g0Hero.IncreasePower(2); //One increase
            g1Hero.IncreasePower(2);
            g2Hero.IncreasePower(3);

            //Assert
            Assert.Equal(g0Hero.value, g1Hero.value);
            Assert.NotEqual(g1Hero.value, g2Hero.value);

            //Act
            var childCont1 = _di.CreateChildContainer("newChild1");
            var g0Child = (SuperHero)childCont1.Resolve<IPerson>("G0"); //Should be new.
            var g1Child = (SuperHero)childCont1.Resolve<IPerson>("G1"); //Should be new.
            var g2Child = (SuperHero)childCont1.Resolve<IPerson>("G2"); //Should not be new.

            g0Child.IncreasePower(1);
            g1Hero.IncreasePower(1); //Increase to 3.
            g1Child.IncreasePower(1); //Will be new and vlaue is at 1
            g2Child.IncreasePower(1);

            Assert.Equal(g2Hero.value, g2Child.value);
            Assert.Equal(g0Child.value, g1Child.value); //Since both are new instance, both have started afresh (1) increase power.

            //Act
            var g2ChildRegister = childCont1.RegisterWithKey<IPerson, SuperHero>("G2"); //if this should not register.
            Assert.False(g2ChildRegister);

            //child should not allow registering universal singleton.
            var exception = Assert.Throws<ArgumentException>(() => { childCont1.RegisterWithKey<IPerson, SuperHero>("G2",RegisterMode.UniversalSingleton); });
        }
    }
}
