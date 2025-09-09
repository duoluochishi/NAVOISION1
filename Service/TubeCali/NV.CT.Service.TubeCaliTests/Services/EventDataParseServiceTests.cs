using Microsoft.VisualStudio.TestTools.UnitTesting;
using NV.CT.Service.TubeCali.Enums;
using NV.CT.Service.TubeCali.Services;
using NV.CT.Service.TubeCali.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.Service.TubeCali.Services.Tests
{
    [TestClass()]
    public class EventDataParseServiceTests
    {
        [TestMethod()]
        public void ParseTubeStatusTest_Doing()
        {
            using var mock = Autofac.Extras.Moq.AutoMock.GetLoose();
            var configServiceMock = mock.Mock<IConfigService>();
            configServiceMock.Setup(p => p.GetTubeAndTubeInterfaceCount())
                .Returns(() => (24, 6, 4));
            //var addressService = new AddressService(configServiceMock.Object);
            var parseService = mock.Create<EventDataParseService>();
            //var parseService = new EventDataParseService(addressService);

            var data = new byte[] { 0x00, 0x06, 0xA2, 0x68, 0x00, 0x00, 0x00, 0x40 };
            Assert.IsTrue(parseService.ParseTubeStatus(data, out var tube));
            Assert.AreEqual(tube.Status, ComponentCaliStatus.Working);
        }

        [TestMethod()]
        public void ParseTubeStatusTest_Success()
        {
            using var mock = Autofac.Extras.Moq.AutoMock.GetLoose();
            var configServiceMock = mock.Mock<IConfigService>();
            configServiceMock.Setup(p => p.GetTubeAndTubeInterfaceCount())
                .Returns(() => (24, 6, 4));
            //var addressService = new AddressService(configServiceMock.Object);
            var parseService = mock.Create<EventDataParseService>();
            //var parseService = new EventDataParseService(addressService);

            var data = new byte[] { 0x00, 0x06, 0xA2, 0x68, 0x00, 0x00, 0x00, 0x80 };
            Assert.IsTrue(parseService.ParseTubeStatus(data, out var tube));
            Assert.AreEqual(tube.Status, ComponentCaliStatus.Success);
        }

        [TestMethod()]
        public void ParseTubeStatusTest_Failed()
        {
            using var mock = Autofac.Extras.Moq.AutoMock.GetLoose();
            var configServiceMock = mock.Mock<IConfigService>();
            configServiceMock.Setup(p => p.GetTubeAndTubeInterfaceCount())
                .Returns(() => (24, 6, 4));
            //var addressService = new AddressService(configServiceMock.Object);
            var parseService = mock.Create<EventDataParseService>();
            //var parseService = new EventDataParseService(addressService);

            var data = new byte[] { 0x00, 0x06, 0xA2, 0x68, 0x00, 0x00, 0x01, 00 };
            Assert.IsTrue(parseService.ParseTubeStatus(data, out var tube));
            Assert.AreEqual(tube.Status, ComponentCaliStatus.Failed);
        }

        [TestMethod()]
        public void ParseTubeStatusTest_Doing_Tube5()
        {
            using var mock = Autofac.Extras.Moq.AutoMock.GetLoose();
            var configServiceMock = mock.Mock<IConfigService>();
            configServiceMock.Setup(p => p.GetTubeAndTubeInterfaceCount())
                .Returns(() => (24, 6, 4));
            //var addressService = new AddressService(configServiceMock.Object);
            var parseService = mock.Create<EventDataParseService>();
            //var parseService = new EventDataParseService(addressService);

            var data = new byte[] { 0x00, 0x07, 0xA2, 0x68, 0x00, 0x00, 0x00, 0x40 };
            Assert.IsTrue(parseService.ParseTubeStatus(data, out var tube));
            Assert.AreEqual(tube.Status, ComponentCaliStatus.Working);
            Assert.AreEqual(tube.TubeNumber, 5);
        }

        [TestMethod]
        public void ParseVoltageStatus_Tube1()
        {
            using var mock = Autofac.Extras.Moq.AutoMock.GetLoose();
            var configServiceMock = mock.Mock<IConfigService>();
            configServiceMock.Setup(p => p.GetTubeAndTubeInterfaceCount())
                .Returns(() => (24, 6, 4));
            var parseService = mock.Create<EventDataParseService>();

            var data = new byte[] { 0x00, 0x06, 0xA1, 0x24, 0x00, 0x00, 0x00, 0x01 };
            var res = parseService.ParseVoltageStatus(data, out var tube);

            Assert.IsTrue(res);
        }

        [TestMethod]
        public void ParseVoltageStatus_Tube5()
        {
            using var mock = Autofac.Extras.Moq.AutoMock.GetLoose();
            var configServiceMock = mock.Mock<IConfigService>();
            configServiceMock.Setup(p => p.GetTubeAndTubeInterfaceCount())
                .Returns(() => (24, 6, 4));
            var parseService = mock.Create<EventDataParseService>();

            var data = new byte[] { 0x00, 0x07, 0xA1, 0x24, 0x00, 0x00, 0x00, 0x01 };
            var res = parseService.ParseVoltageStatus(data, out var tube);

            Assert.IsTrue(res);
        }

        [TestMethod]
        public void ParseCurrentStatus_Tube1()
        {
            using var mock = Autofac.Extras.Moq.AutoMock.GetLoose();
            var configServiceMock = mock.Mock<IConfigService>();
            configServiceMock.Setup(p => p.GetTubeAndTubeInterfaceCount())
                .Returns(() => (24, 6, 4));
            var parseService = mock.Create<EventDataParseService>();

            var data = new byte[] { 0x00, 0x06, 0xA0, 0xDC, 0x00, 0x00, 0x00, 0x01 };
            var res = parseService.ParseCurrentStatus(data, out var tube);

            Assert.IsTrue(res);
        }

        [TestMethod]
        public void ParseCurrentStatus_Tube5()
        {
            using var mock = Autofac.Extras.Moq.AutoMock.GetLoose();
            var configServiceMock = mock.Mock<IConfigService>();
            configServiceMock.Setup(p => p.GetTubeAndTubeInterfaceCount())
                .Returns(() => (24, 6, 4));
            var parseService = mock.Create<EventDataParseService>();

            var data = new byte[] { 0x00, 0x07, 0xA0, 0xDC, 0x00, 0x00, 0x00, 0x01 };
            var res = parseService.ParseCurrentStatus(data, out var tube);

            Assert.IsTrue(res);
        }

        [TestMethod]
        public void ParseMsStatus_Tube1()
        {
            using var mock = Autofac.Extras.Moq.AutoMock.GetLoose();
            var configServiceMock = mock.Mock<IConfigService>();
            configServiceMock.Setup(p => p.GetTubeAndTubeInterfaceCount())
                .Returns(() => (24, 6, 4));
            var parseService = mock.Create<EventDataParseService>();

            var data = new byte[] { 0x00, 0x06, 0xA1, 0x6C, 0x00, 0x00, 0x00, 0x01 };
            var res = parseService.ParseMsStatus(data, out var tube);

            Assert.IsTrue(res);
        }

        [TestMethod]
        public void ParseMsStatus_Tube5()
        {
            using var mock = Autofac.Extras.Moq.AutoMock.GetLoose();
            var configServiceMock = mock.Mock<IConfigService>();
            configServiceMock.Setup(p => p.GetTubeAndTubeInterfaceCount())
                .Returns(() => (24, 6, 4));
            var parseService = mock.Create<EventDataParseService>();

            var data = new byte[] { 0x00, 0x07, 0xA1, 0x6C, 0x00, 0x00, 0x00, 0x01 };
            var res = parseService.ParseMsStatus(data, out var tube);

            Assert.IsTrue(res);
        }
    }
}