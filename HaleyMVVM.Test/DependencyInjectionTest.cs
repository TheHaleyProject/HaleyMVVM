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
        IBaseContainer _diSingleton = ContainerStore.Singleton.DI;
        [Fact]
        public void Concrete__Equals()
        {
            //Arrange
            IBaseContainer _di = new DIContainer();
            Person p_expected = new Person() { name = "Latha G" };
            _di.Register<Person>(p_expected);

            //Act
            var p_actual = _di.Resolve<Person>();

            //Assert
            Assert.Equal(p_expected, p_actual); //If not registered, this should be equal to what we send.
        }

        [Fact]
        public void Concrete_NotEquals()
        {
            //Arrange
            IBaseContainer _di = new DIContainer();
            Person p_expected = new Person() { name = "Senguttuvan" };
            _di.Register<Person>(p_expected);

            //Act
            var p_actual = _di.Resolve<Person>(ResolveMode.Transient); //Since generating new instance, this should not be equal

            //Assert
            Assert.NotEqual(p_expected, p_actual);
        }

        [Fact]
        public void ForcedSingeltonCheck()
        {
            //Arrange
            IBaseContainer _di = new DIContainer();
            string basename = "Pranav Krishna";
            Person p_expected = new Person() { name = basename };
            _di.Register<Person>(p_expected,true); //Registering as forced singleton. So, even if transient is requested, it should always give pranavkrishna

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
            IContainerFactory _factory = new ContainerFactory(new DIContainer()); //This should register itself, basecontainer, uicontainer, and control container.

            //Set 02
            IBaseContainer _newbase = new DIContainer();
            IControlContainer _newControl = new ControlContainer(_newbase);
            IWindowContainer _newWndw = new WindowContainer(_newbase);

            //var _houseFactory01 = ((IBaseContainer)_factory.DI).Resolve<HouseFactory>(); //This should have all relevance to main factory.

            var _houseFactory01 = ((ContainerFactory)_factory).GetDI().Resolve<HouseFactory>(); //This should have all relevance to main factory.


            //Act
            var _house02 = _newbase.Resolve<House>(); //resolve using newbasecontainer
            var _oldhouse = ((IBaseContainer)_factory.Services).Resolve<House>(); //resolve using the main factory.

            //Assert
            Assert.Equal(_houseFactory01.house.Container.Id, _oldhouse.Container.Id); //Both houses should have received same base container.
            Assert.Equal(_newbase.Id, _house02.Container.Id); //New house has received new id.
            Assert.NotEqual(_houseFactory01.house.Container.Id, _house02.Container.Id); //Both houses resolved using different containers should have different ids.

        }

        [Fact]
        public void Concrete_CustomMapping()
        {
            //Arrange
            IBaseContainer _di = new DIContainer();
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
            IBaseContainer _di = new DIContainer();
            IPerson p1 = new SuperHero() { name = "Bruce Wayne", alter_ego="BatMan" };

            //Act
            Action act = () => _di.Register<IPerson>(p1);

            //Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(act);
            Assert.StartsWith("Concrete type cannot be null, abstr", exception.Message.Substring(0,35));
        }

        [Fact]
        public void TypeMapping_Equals_Singleton()
        {
            //Arrange
            IBaseContainer _di = new DIContainer();
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
            IBaseContainer _di = new DIContainer();
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
            IBaseContainer _di = new DIContainer();
            string power = "Money";
            MappingProviderBase _mpb = new MappingProviderBase();
            _mpb.Add<string>(nameof(SuperHero.power), power, typeof(SuperHero), InjectionTarget.Property);
            //Act
            _di.Register<IPerson, SuperHero>();
            var _shero = (SuperHero)_di.ResolveTransient<IPerson>(_mpb,MappingLevel.CurrentWithDependencies);
            
            //Assert
            Assert.Equal(power, _shero.power);
        }

        [Fact]
        public void TypeMapping__IMapping_Register()
        {
            //Arrange
            IBaseContainer _di = new DIContainer();
            string power = "Money";
            MappingProviderBase _mpb = new MappingProviderBase();
            _mpb.Add<string>(nameof(SuperHero.power), power, typeof(SuperHero), InjectionTarget.Property);
            //Act
            _di.Register<IPerson, SuperHero>(_mpb,MappingLevel.Current);
            var _shero = (SuperHero)_di.Resolve<IPerson>();


            //Assert
            Assert.Equal(power, _shero.power);
        }
    }
}
