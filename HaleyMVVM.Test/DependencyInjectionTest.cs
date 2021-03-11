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
        IHaleyDIContainer _diSingleton = ContainerStore.Singleton.DI;
        [Fact]
        public void Concrete__Equals()
        {
            //Arrange
            IHaleyDIContainer _di = new DIContainer();
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
            IHaleyDIContainer _di = new DIContainer();
            Person p_expected = new Person() { name = "Senguttuvan" };
            _di.Register<Person>(p_expected);

            //Act
            var p_actual = _di.Resolve<Person>(ResolveMode.Transient); //Since generating new instance, this should not be equal

            //Assert
            Assert.NotEqual(p_expected, p_actual);
        }

        [Fact]
        public void Concrete_CustomMapping()
        {
            //Arrange
            IHaleyDIContainer _di = new DIContainer();
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
            IHaleyDIContainer _di = new DIContainer();
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
            IHaleyDIContainer _di = new DIContainer();
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
            IHaleyDIContainer _di = new DIContainer();
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
            IHaleyDIContainer _di = new DIContainer();
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
            IHaleyDIContainer _di = new DIContainer();
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
