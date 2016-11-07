using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nelson.Architecture.SingleResponsibility.Core
{
    public class MailerMessage { }
    public class Mailer
    {
        public void SendMail(MailerMessage message)
        {
            if (message == null)
                throw new MailerException();
        }
    }
    public class MailerException : Exception { }
}
