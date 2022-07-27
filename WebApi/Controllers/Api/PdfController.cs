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

            var decryptedXml = await this.SignAndSendXmlRequestToSoapService();

            return Ok(decryptedXml);
        }

        

        [HttpGet]
        [Route("signSoapHeader")]
        public async Task<IHttpActionResult> SignSOAPHeader()
        {
            var cert = this.GetCertificateByThumbPrint();
            var signedXml = this.BuildEnvelope(cert);
            await Task.CompletedTask;
            
            //var decryptedXml = await this.SignAndSendXmlRequestToSoapService();

            return Ok(signedXml);
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
        private async Task<string> SignAndSendXmlRequestToSoapService()
        {
            string decryptedXml = null;

            var creditCard = new CreditCard
            {
                CardNumber = "12345678910",
                ExpiryDate = DateTime.Today.AddYears(7)
            };

            var requestContent = new CreditCardContainer
            {
                CardDetails = creditCard
            };
           

            var xmlPayload = requestContent.XmlSerialize();

            //digitally sign the xml content            
            

            var encryptedXml = this.EncryptXml(xmlPayload);

            
            var requestUri = "https://localhost:44323/api/utility/decryptXml";            

            var xmlString  = requestContent.XmlSerialize();

            requestContent.EncryptedXml = encryptedXml;

            var finalXml = requestContent.XmlSerialize();

            using (HttpContent content = new StringContent(finalXml, Encoding.UTF8, "text/xml"))
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri))
            {               
                request.Content = content;
                using (HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode(); // throws an Exception if 404, 500, etc.
                    decryptedXml = await response.Content.ReadAsStringAsync();
                }
            }

            return decryptedXml;
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
            var encryptedXml = EncryptXml(xmlDoc, "CardDetails", cert);

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
            EncryptedXml.ReplaceElement(elementToEncrypt, edElement, true);

            var xmlElement = edElement.GetXml().OuterXml;

            return xmlElement;
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
            string thumbPrint = "42d3c8c8c21afe794b889f67016f093b2454a830"; //b442dddc1d642ecef0d301d3f3067bd3f74a7874

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

            catch (Exception ex)
            {
                var message = ex.Message;
            }
            finally
            {
                // Close the store even if an exception was thrown.
                store.Close();
            }

            return cert;
        }

        /// <summary>
        /// https://stackoverflow.com/questions/47104618/how-do-i-call-xml-soap-service-that-requires-signature-from-net-core
        /// This is manually creating the  SOAP envelope ...If at all possible, don't do this
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns></returns>
        private string BuildEnvelope(X509Certificate2 certificate)
        {
            string envelope = null;
            // note - lots of bits here specific to my thirdparty
            string cert_id = string.Format("uuid-{0}-1", Guid.NewGuid().ToString());
            using (var stream = new MemoryStream())
            {
                Encoding utf8 = new UTF8Encoding(false); // omit BOM
                using (var writer = new XmlTextWriter(stream, utf8))
                {
                    // timestamp
                    DateTime dt = DateTime.UtcNow;
                    string now = dt.ToString("o").Substring(0, 23) + "Z";
                    string plus5 = dt.AddMinutes(5).ToString("o").Substring(0, 23) + "Z";

                    // soap envelope
                    // <s:Envelope xmlns:s="http://www.w3.org/2003/05/soap-envelope" xmlns:a="http://www.w3.org/2005/08/addressing" xmlns:u="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd">
                    writer.WriteStartDocument();
                    writer.WriteStartElement("s", "Envelope", "http://www.w3.org/2003/05/soap-envelope");
                    writer.WriteAttributeString("xmlns", "a", null, "http://www.w3.org/2005/08/addressing");
                    writer.WriteAttributeString("xmlns", "u", null, "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");

                    writer.WriteStartElement("s", "Header", null);

                    /////////////////
                    //  saml guts  //
                    /////////////////

                    //<a:Action s:mustUnderstand="1">http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue</a:Action>
                    writer.WriteStartElement("a", "Action", null);
                    writer.WriteAttributeString("s", "mustUnderstand", null, "1");
                    writer.WriteString("http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue");
                    writer.WriteEndElement(); //Action

                    //<a:MessageID>urn:uuid:0cc426dd-35bf-4c8b-a737-7e2ae94bd44d</a:MessageID>
                    string msg_id = string.Format("urn:uuid:{0}", Guid.NewGuid().ToString());
                    writer.WriteStartElement("a", "MessageID", null);
                    writer.WriteString(msg_id);
                    writer.WriteEndElement(); //MessageID

                    //<a:ReplyTo><a:Address>http://www.w3.org/2005/08/addressing/anonymous</a:Address></a:ReplyTo>
                    writer.WriteStartElement("a", "ReplyTo", null);
                    writer.WriteStartElement("a", "Address", null);
                    writer.WriteString("http://www.w3.org/2005/08/addressing/anonymous");
                    writer.WriteEndElement(); //Address
                    writer.WriteEndElement(); //ReplyTo

                    writer.WriteStartElement("a", "To", "http://www.w3.org/2005/08/addressing");
                    writer.WriteAttributeString("s", "mustUnderstand", null, "1");
                    writer.WriteAttributeString("u", "Id", null, "_1");
                    writer.WriteString("https://thirdparty.com/service.svc");
                    writer.WriteEndElement(); //To

                    //<o:Security xmlns:o="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd" s:mustUnderstand="1">
                    writer.WriteStartElement("o", "Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
                    writer.WriteAttributeString("s", "mustUnderstand", null, "1");

                    //<u:Timestamp u:Id="_0">
                    writer.WriteStartElement("u", "Timestamp", null);
                    writer.WriteAttributeString("u", "Id", null, "_0");

                    //<u:Created>2018-02-08T15:03:13.115Z</u:Created>
                    writer.WriteElementString("u", "Created", null, now);

                    //<u:Expires>2018-02-08T15:08:13.115Z</u:Expires>
                    writer.WriteElementString("u", "Expires", null, plus5);

                    writer.WriteEndElement(); //Timestamp

                    writer.WriteStartElement("o", "BinarySecurityToken", null);
                    writer.WriteAttributeString("u", "Id", null, cert_id);
                    writer.WriteAttributeString("ValueType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3");
                    writer.WriteAttributeString("EncodingType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary");
                    byte[] rawData = certificate.GetRawCertData();
                    writer.WriteBase64(rawData, 0, rawData.Length);
                    writer.WriteEndElement(); //BinarySecurityToken

                    writer.WriteEndElement(); //Security
                    writer.WriteEndElement(); //Header

                    //<s:Body xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
                    writer.WriteStartElement("s", "Body", "http://www.w3.org/2003/05/soap-envelope");
                    writer.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
                    writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");

                    // your 3rd-party soap payload goes here
                    //writer.WriteStartElement("???", "http://docs.oasis-open.org/ws-sx/ws-trust/200512");
                    // ...                
                    //writer.WriteEndElement(); // 
                    writer.WriteEndElement(); // Body


                    writer.WriteEndElement(); //Envelope
                }

                // signing pass
                var signable = Encoding.UTF8.GetString(stream.ToArray());
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(signable);

                // see https://stackoverflow.com/a/6467877
                var signedXml = new SignedXmlWithId(doc);

                var key = certificate.GetRSAPrivateKey();
                signedXml.SigningKey = key;
                // these values may not be supported by your 3rd party - they may use e.g. SHA256 miniumum
                signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;
                signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA1Url;

                // 
                KeyInfo keyInfo = new KeyInfo();
                KeyInfoX509Data x509data = new KeyInfoX509Data(certificate);
                keyInfo.AddClause(x509data);
                signedXml.KeyInfo = keyInfo;

                // 3rd party wants us to only sign the timestamp fragment- ymmv
                Reference reference0 = new Reference();
                reference0.Uri = "#_0";
                var t0 = new XmlDsigExcC14NTransform();
                reference0.AddTransform(t0);
                reference0.DigestMethod = SignedXml.XmlDsigSHA1Url;
                signedXml.AddReference(reference0);
                // etc

                // get the sig fragment
                signedXml.ComputeSignature();
                XmlElement xmlDigitalSignature = signedXml.GetXml();

                // modify the fragment so it points at BinarySecurityToken instead
                XmlNode info = null;
                for (int i = 0; i < xmlDigitalSignature.ChildNodes.Count; i++)
                {
                    var node = xmlDigitalSignature.ChildNodes[i];
                    if (node.Name == "KeyInfo")
                    {
                        info = node;
                        break;
                    }
                }
                info.RemoveAll();

                // 
                XmlElement securityTokenReference = doc.CreateElement("o", "SecurityTokenReference", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
                XmlElement reference = doc.CreateElement("o", "Reference", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
                reference.SetAttribute("ValueType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3");
                // cert id                
                reference.SetAttribute("URI", "#" + cert_id);
                securityTokenReference.AppendChild(reference);
                info.AppendChild(securityTokenReference);

                var nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("o", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
                nsmgr.AddNamespace("s", "http://www.w3.org/2003/05/soap-envelope");
                var security_node = doc.SelectSingleNode("/s:Envelope/s:Header/o:Security", nsmgr);
                security_node.AppendChild(xmlDigitalSignature);

                envelope = doc.OuterXml;
            }

            return envelope;
        }


    }


    public class SignedXmlWithId : SignedXml
    {
        public SignedXmlWithId(XmlDocument xml) : base(xml)
        {
        }

        public SignedXmlWithId(XmlElement xmlElement)
            : base(xmlElement)
        {
        }

        public override XmlElement GetIdElement(XmlDocument doc, string id)
        {
            // check to see if it's a standard ID reference
            XmlElement idElem = base.GetIdElement(doc, id);

            if (idElem == null)
            {
                XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
                nsManager.AddNamespace("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");

                idElem = doc.SelectSingleNode("//*[@wsu:Id=\"" + id + "\"]", nsManager) as XmlElement;
            }

            return idElem;
        }
    }

}
