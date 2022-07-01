using Aspose.Pdf;
using Aspose.Pdf.Forms;
using Newtonsoft.Json;
using PdfSharp.Pdf;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml;
using System.Xml.Serialization;
using WebApi.Helpers;
using WebApi.Models;

namespace WebApi.Controllers.Api
{
    /// <summary>
    /// https://blog.aspose.com/2020/05/25/create-fill-edit-fillable-pdf-form-csharp/
    /// https://www.c-sharpcorner.com/article/fill-in-pdf-form-fields-using-the-open-source-itextsharp-dll/
    /// https://www.c-sharpcorner.com/article/fill-in-pdf-form-fields-using-the-open-source-itextsharp-dll/
    /// </summary>
    /// 
    [RoutePrefix("api/pdf")]
    public class PdfController : ApiController
    {
        private static HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Loads a pdf from file syste
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("create")]
        public async Task<IHttpActionResult> CreatePdf([FromBody] PdfData request)
        {
            //await FillPdfTemplateUsingAspose();

            // await FillPdfTemplateUsingFreeSpirePdf();

            await FillPdfTemplateUsingAbsolutelyFreePdfSharp();

            await Task.CompletedTask;
            return Ok();
        }

        [HttpGet]
        [Route("signXml")]
        public async Task<IHttpActionResult> SignXml()
        {
            await Task.CompletedTask;

            await this.SignAndSendXmlRequestToSoapService();

            return Ok();
        }

        private async Task FillPdfTemplateUsingAspose()
        {
            /* step1: download pdftoolkit comannd line
                         * step 2: run this commsn --> pdftk C:\Development\Truist-TPot\TestHarness\TPotSupportingApp\WebApi\PdfTemplate\OoPdfFormExample.pdf dump_data_fields > "C:\Development\Truist-TPot\eSign\pdfFillableFiledName"
                         * step 3: This will dump all fields
                         * https://products.aspose.com/pdf/net/
                           from Amit H to everyone:    9:23 AM
                           https://www.e-iceblue.com/Buy/Spire.PDF.html
                           from Rennish Joseph to everyone:    9:24 AM
                           https://www.nuget.org/packages/iTextSharp/ --> Deprecated
                           from Amit H to everyone:    9:29 AM
                           https://nugetmusthaves.com/Package/sautinsoft.document

                           https://blog.aspose.com/2020/11/29/convert-file-byte-array-pdf-csharp/
                         * 
                         * 
                         * 
                         * */
            //finding fillable fields on a pdf form
            //https://stackoverflow.com/questions/2127878/extract-pdf-form-field-names-from-a-pdf-form

            //https://www.pdflabs.com/tools/pdftk-the-pdf-toolkit/

            //https://blog.aspose.com/2020/05/25/create-fill-edit-fillable-pdf-form-csharp/#section2
            /* Loads a pdf from file system
             * 
             * 
             * 
             * */
            // Open document
            var pdfTemplate = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"PdfTemplate\OoPdfFormExample.pdf");
            Document pdfDocument = new Document(pdfTemplate);
            // Get the fields
            TextBoxField textBoxField1 = pdfDocument.Form["Given Name Text Box"] as TextBoxField;
            //TextBoxField textBoxField2 = pdfDocument.Form["Family Name Text Box"] as TextBoxField;
            // Fill form fields' values
            textBoxField1.Value = "Rennish";
            textBoxField1.ReadOnly = true;
            //textBoxField2.Value = "Joseph";

            // Get radio button field
            //RadioButtonField radioField = pdfDocument.Form["radio"] as RadioButtonField;
            // Specify the index of radio button from group
            // radioField.Selected = 1;

            //dataDir = dataDir + "Fill_PDF_Form_Field.pdf";
            // Save updated document
            pdfDocument.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"PdfTemplate\OoPdfFormExample_modified.pdf"));
            await PostPdfDocumentToApi(pdfDocument);
        }

        /// <summary>
        /// This needs licensing
        /// </summary>
        /// <returns></returns>
        private async Task FillPdfTemplateUsingFreeSpirePdf()
        {
            // https://www.e-iceblue.com/Tutorials/Spire.PDF/Spire.PDF-Program-Guide/PDF-FormField/Fill-Form-Fields-in-PDF-File-with
            var pdfTemplatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"PdfTemplate\OoPdfFormExample.pdf");

            PdfSharp.Pdf.PdfDocument pdfDocument = PdfReader.Open(pdfTemplatePath, PdfDocumentOpenMode.Modify);
            PdfAcroForm form = pdfDocument.AcroForm;

            if (form.Elements.ContainsKey("/NeedAppearances"))
            {
                form.Elements["/NeedAppearances"] = new PdfSharp.Pdf.PdfBoolean(true);
            }
            else
            {
                form.Elements.Add("/NeedAppearances", new PdfSharp.Pdf.PdfBoolean(true));
            }

            PdfTextField firstName = (PdfTextField)(form.Fields["Given Name Text Box"]);
            firstName.Text = "Rennish";
            firstName.ReadOnly = true;

            PdfTextField lastName = (PdfTextField)(form.Fields["Family Name Text Box"]);
            lastName.Text = "Joseph";
            lastName.ReadOnly = true;

            PdfTextField address1 = (PdfTextField)(form.Fields["House nr Text Box"]);
            address1.Text = "Joseph";
            address1.ReadOnly = true;

            

            var newFileName = string.Format("{0}_{1}.pdf", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"PdfTemplate\\OoPdfFormExample_modified"), $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}");
            pdfDocument.Save(newFileName);
            pdfDocument.Close();
            
            await PostPdfDocumentToApiLatest_V2(newFileName);
        }

        /// <summary>
        /// This is free version.
        /// https://stackoverflow.com/questions/6240585/pdfsharp-filling-in-form-fields
        /// </summary>
        /// <returns></returns>
        private async Task FillPdfTemplateUsingAbsolutelyFreePdfSharp()
        {
            /* step 1: download tool from here https://www.pdflabs.com/tools/pdftk-the-pdf-toolkit/
             * step2: run this in the pdf template directory: sample.pdf "path_to_pdf_template" dump_data_fields > fields.txt
             * step3: code against the FieldName from the output file.
             * */

            // https://stackoverflow.com/questions/6240585/pdfsharp-filling-in-form-fields
            var pdfTemplatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"PdfTemplate\OoPdfFormExample.pdf");

            PdfSharp.Pdf.PdfDocument pdfDocument = PdfReader.Open(pdfTemplatePath, PdfDocumentOpenMode.Modify);
            PdfAcroForm form = pdfDocument.AcroForm;

            if (form.Elements.ContainsKey("/NeedAppearances"))
            {
                form.Elements["/NeedAppearances"] = new PdfSharp.Pdf.PdfBoolean(true);
            }
            else
            {
                form.Elements.Add("/NeedAppearances", new PdfSharp.Pdf.PdfBoolean(true));
            }            

            form.Fields["Given Name Text Box"].Value = new PdfString("Rennish");
            form.Fields["Given Name Text Box"].ReadOnly = true;

            form.Fields["Family Name Text Box"].Value = new PdfString("Joseph");
            form.Fields["Family Name Text Box"].ReadOnly = true;

            form.Fields["House nr Text Box"].Value = new PdfString("1068 Candlebark Dr");
            form.Fields["House nr Text Box"].ReadOnly = true;

            var newFileName = string.Format("{0}_{1}.pdf", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"PdfTemplate\\OoPdfFormExample_modified"), $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}");
            pdfDocument.Save(newFileName);
            pdfDocument.Close();

            await PostPdfDocumentToApiLatest_V2(newFileName);
        }


        /// <summary>
        /// https://brokul.dev/sending-files-and-additional-data-using-httpclient-in-net-core
        /// </summary>
        /// <param name="pdfBytes"></param>
        /// <returns></returns>
        private async Task PostPdfDocumentToApiLatest_V2(string filePath)
        {

            var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:44323/api/")
            };

            using (var stream = File.OpenRead(filePath))
            {
                var payLoad = new
                {
                    ConfirmationNumber = "F79364EE-E538-46D7-9CAA-FEA5003EEA96"
                };

                var fileName = Path.GetFileName(filePath);
                byte[] pdfBytes = null;
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    pdfBytes = memoryStream.ToArray();
                }
                var base64Bytes = Convert.ToBase64String(pdfBytes);
                var content = new MultipartFormDataContent
                    {
                      {new StringContent(base64Bytes, Encoding.UTF8, "application/pdf"), "PdfFile", fileName },
                        // file
                        //{ new StreamContent(stream), "PdfFile", fileName},
                        //payload
                      {new StringContent(payLoad.ConfirmationNumber), "ConfirmationNumber" }
                    };
                var request = new HttpRequestMessage(HttpMethod.Post, "buggy/readFile");
                request.Content = content;
                var response = await client.SendAsync(request);
            }

            await Task.CompletedTask;
        }

        private async Task PostPdfDocumentToApi<T>(T pdfDocument)
        {
            var pdfBytes = this.ConvertPdfToByteArray(pdfDocument);

            var byteArrayContent = new ByteArrayContent(pdfBytes);
            byteArrayContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

            var pdfData = new PdfData
            {
                ConfirmationNumber = "F79364EE-E538-46D7-9CAA-FEA5003EEA96"
            };

            var xmlString = pdfData.XmlSerialize();

            var response = await httpClient.PostAsync(@"https://localhost/api/buggy/files/readFile", new MultipartFormDataContent
            {
                //Form Data
                { new StringContent(xmlString)},
                { byteArrayContent}
            });
        }

        private async Task PostPdfDocumentToApiLatest(byte[] pdfBytes)
        {            

            var byteArrayContent = new ByteArrayContent(pdfBytes);
            byteArrayContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var pdfData = new PdfData
            {
                ConfirmationNumber = "F79364EE-E538-46D7-9CAA-FEA5003EEA96"
            };

            //var xmlString = XmlSerialize(pdfData);

            var jsonString = JsonConvert.SerializeObject(pdfData);

            var response = await httpClient.PostAsync(@"https://localhost:44323/api/buggy/readFile", new MultipartFormDataContent
            {
                //Form Data
                { new StringContent(jsonString)},
                { byteArrayContent}
            });

            await Task.CompletedTask;
        }

        /// <summary>
        /// Takes an object and converts that to byte[]
        /// </summary>
        /// <param name="pdfDocument"></param>
        /// <returns></returns>
        private byte[] ConvertPdfToByteArray<T>(T pdfDocument)
        {
            try
            {                
                var xml = pdfDocument.XmlSerialize();
                return System.Text.Encoding.UTF8.GetBytes(xml);
            }
            catch (Exception ex)
            {

                throw;
            }
        }



        /// <summary>
        /// Create the xml object, digitally sign the xml and send to soap api.
        /// </summary>
        /// <returns></returns>
        private async Task SignAndSendXmlRequestToSoapService()
        {
            var personLists = new List<Person>
            {
                new Person{ Id  =1, FirstName = "Rennish", LastName = "Joseph", Email = "rjoseph@email.com", CreditCardNumber = "1234567891011"},
                new Person{ Id  =2, FirstName = "Rennish", LastName = "Joseph", Email = "rjoseph@email.com",  CreditCardNumber = "1234567891011"},
                new Person{ Id  =3, FirstName = "Rennish", LastName = "Joseph", Email = "rjoseph@email.com", CreditCardNumber = "1234567891011"},
                new Person{ Id  =4, FirstName = "Rennish", LastName = "Joseph", Email = "rjoseph@email.com", CreditCardNumber = "1234567891011"},
                new Person{ Id  =5, FirstName = "Rennish", LastName = "Joseph", Email = "rjoseph@email.com" , CreditCardNumber = "1234567891011"}
            };

            var personXml = personLists.XmlSerialize();

            //digitally sign the xml content
            var signedXml = this.EncryptXml(personXml);
            using (HttpContent content = new StringContent(signedXml, Encoding.UTF8, "text/xml"))
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://localhost/api/badrequest"))
            {
                request.Headers.Add("SOAPAction", "");
                request.Content = content;
                using (HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    //response.EnsureSuccessStatusCode(); // throws an Exception if 404, 500, etc.
                     await response.Content.ReadAsStringAsync();
                }
            }
        }

        /// <summary>
        /// https://www.wiktorzychla.com/2012/12/interoperable-xml-digital-signatures-c_20.html --> This code is taken from here
        /// https://stackoverflow.com/questions/4666970/signing-soap-messages-using-x-509-certificate-from-wcf-service-to-java-webservic
        /// https://www.scottbrady91.com/c-sharp/xml-signing-dotnet
        /// https://docs.microsoft.com/en-us/dotnet/standard/security/how-to-encrypt-xml-elements-with-asymmetric-keys
        /// </summary>
        /// <param name="xmlString"></param>
        /// <param name="SubjectName"></param>
        /// <param name="Signature"></param>
        /// <param name="keyInfoRefId"></param>
        /// <returns></returns>
        private string EncryptXml(string xmlString)
        {
            // Create an XmlDocument object.
            XmlDocument xmlDoc = new XmlDocument();

            // Load an XML file into the XmlDocument object.
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.LoadXml(xmlString);

            var cert = this.GetCertificateByThumbPrint();
            var encryptedXml = EncryptXml(xmlDoc, "ArrayOfPerson", cert);

            return encryptedXml;

        }

        private string EncryptXml(XmlDocument xmlDoc, string ElementToEncrypt, X509Certificate2 Cert)
        {
            ////////////////////////////////////////////////
            // Find the specified element in the XmlDocument
            // object and create a new XmlElement object.
            ////////////////////////////////////////////////

            XmlElement elementToEncrypt = xmlDoc.GetElementsByTagName(ElementToEncrypt)[0] as XmlElement;
            // Throw an XmlException if the element was not found.
            if (elementToEncrypt == null)
            {
                throw new XmlException("The specified element was not found");
            }

            //////////////////////////////////////////////////
            // Create a new instance of the EncryptedXml class
            // and use it to encrypt the XmlElement with the
            // X.509 Certificate.
            //////////////////////////////////////////////////

            EncryptedXml eXml = new EncryptedXml();

            // Encrypt the element.
            EncryptedData edElement = eXml.Encrypt(elementToEncrypt, Cert);

            ////////////////////////////////////////////////////
            // Replace the element from the original XmlDocument
            // object with the EncryptedData element.
            ////////////////////////////////////////////////////
            EncryptedXml.ReplaceElement(elementToEncrypt, edElement, false);

            return edElement.GetXml().InnerXml;
        }

        /// <summary>
        /// Loads X509 certificates (pfx) from personal store  for the current user ( this should be for the service account the app is running under)
        /// https://stackoverflow.com/questions/4666970/signing-soap-messages-using-x-509-certificate-from-wcf-service-to-java-webservic
        /// </summary>
        /// <param name="subjectName"></param>
        /// <returns></returns>
        private  X509Certificate2 GetCertificateByThumbPrint()
        {

            // Load the certificate from the certificate store.
            X509Certificate2 cert = null;
            string thumbPrint = "b442dddc1d642ecef0d301d3f3067bd3f74a7874";

            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

            try
            {
                // Open the store.
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

                // Find the certificate with the specified subject.
                cert = store.Certificates.Find(X509FindType.FindByThumbprint, thumbPrint, false)[0];

                // Throw an exception of the certificate was not found.
                if (cert == null)
                {
                    throw new CryptographicException("The certificate could not be found.");
                }
            }
            finally
            {
                // Close the store even if an exception was thrown.
                store.Close();
            }

            return cert;
        }
    }

    


}
