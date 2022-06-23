using Aspose.Pdf;
using Aspose.Pdf.Forms;
using Newtonsoft.Json;
using PdfSharp.Pdf;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.IO;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml;
using System.Xml.Serialization;
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

            var xmlString = XmlSerialize(pdfData);

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
                var xml = XmlSerialize(pdfDocument);
                return System.Text.Encoding.UTF8.GetBytes(xml);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private static string XmlSerialize<T>(T pdfDocument)
        {
            var xml = "";
            var serializer = new XmlSerializer(typeof(T));
            using (var sww = new Utf8StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    serializer.Serialize(writer, pdfDocument);
                    xml = sww.ToString(); // Your XML
                }
            }
            return xml;
            //var jsonString = JsonConvert.SerializeObject(pdfDocument);
            //bf.Serialize(ms, pdfDocument);
            //
        }
    }

    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
