using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Nelson.Architecture.SingleResponsibility.Core
{
    public class MailerTests
    {
        [Fact]
        public void MailerThrowsIfMessageIsNull()
        {
            var sut = new Mailer();
            Assert.Throws<MailerException>(() => sut.SendMail(null));
        }

        [Fact]
        public void 
    }
}
