using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WebApi.Models
{
    //[DataContract]
    public class PdfData
    {
        //[DataMember]
        public string ConfirmationNumber { get; set; }
    }

    public class PersonContainer
    {
        public List<Person> PersonsList { get; set; }
    }

    public class CreditCard
    {
        public string CardNumber { get; set; }
        public DateTime ExpiryDate { get; set; }

    }

    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string CreditCardNumber { get; set; }
    }
    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }

    public class CreditCardContainer
    {
        public CreditCard CardDetails { get; set; }
        public string EncryptedXml { get; set; }

    }

}
