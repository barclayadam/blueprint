//using System.Text;
//using System.Web.Mvc;
//using Microsoft.AspNetCore.Http;
//using Moq;

//namespace Blueprint.Tests.Fakes
//{
//    public class FakeControllerContext : Mock<ControllerContext>
//    {
//        private readonly Mock<HttpResponse> response;
//        private readonly StringBuilder writtenContext;

//        public FakeControllerContext()
//        {
//            writtenContext = new StringBuilder();
//            response = new Mock<HttpResponse>();

//            response.Setup(x => x.Write(It.IsAny<string>())).Callback<string>(y => writtenContext.Append(y));
//            response.SetupAllProperties();

//            Setup(x => x.HttpContext.Response).Returns(response.Object);
//        }

//        public Encoding ContentEncoding => response.Object.ContentEncoding;

//        public string ResponseText => writtenContext.ToString();

//        public static implicit operator ControllerContext(FakeControllerContext controllerContext)
//        {
//            return controllerContext.Object;
//        }
//    }
//}
