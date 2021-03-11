using System;
using Xunit;
using Haley.Events;
using Haley.MVVM;
using HaleyMVVM.Test.Events;
using HaleyMVVM.Test.Models;

namespace HaleyMVVM.Test
{
    public class EventTest
    {
        public object _message { get; set; }
        [Theory]
        [InlineData(9.0)]
        [InlineData(6)]
        [InlineData ("Hello world")]
        public void InvokeandReceiveEvent(object new_message)
        {
            //Arrange
            EventStore.Singleton.GetEvent<MessageEvent>().subscribe(_handleMessage,true);

            //Act
            EventStore.Singleton.GetEvent<MessageEvent>().publish(new_message);

            //Assert
            Assert.Equal(new_message, _message);
        }

        private void _handleMessage(object _input)
        {
            _message = _input;
        }
    }


}
