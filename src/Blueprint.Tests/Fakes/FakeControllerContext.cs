using System.Text;
using System.Web;
using System.Web.Mvc;
using Moq;

namespace Blueprint.Tests.Fakes
{
    public class FakeControllerContext : Mock<ControllerContext>
    {
        private readonly Mock<HttpResponseBase> response;
        private readonly StringBuilder writtenContext;

        public FakeControllerContext()
        {
            writtenContext = new StringBuilder();
            response = new Mock<HttpResponseBase>();

            response.Setup(x => x.Write(It.IsAny<string>())).Callback<string>(y => writtenContext.Append(y));
            response.SetupAllProperties();

            Setup(x => x.HttpContext.Response).Returns(response.Object);
        }

        public Encoding ContentEncoding { get { return response.Object.ContentEncoding; } }

        public string ResponseText { get { return writtenContext.ToString(); } }

        public static implicit operator ControllerContext(FakeControllerContext controllerContext)
        {
            return controllerContext.Object;
        }
    }
}