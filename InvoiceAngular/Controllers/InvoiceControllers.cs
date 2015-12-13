using InvoiceAngular.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http;
using Excel = Microsoft.Office.Interop.Excel;


namespace InvoiceAngular.Controllers
{
    [RoutePrefix("api/invoice")]
    public class InvoiceController : ApiController
    {

        private readonly InvoiceDbContext dbContext;

        public InvoiceController()
        {
            dbContext = new InvoiceDbContext();
        }

        // GET api/Invoice
        [HttpGet]
        // [Authorize]
        public PagedResult<InvoiceViewModel> GetItems(int pageNo = 1, int pageSize = 50, [FromUri] string[] sort = null, string search = null)
        {
            // Determine the number of records to skip
            int skip = (pageNo - 1) * pageSize;


            IQueryable<Invoice> queryable = dbContext.Invoice;

            int count = queryable.Count();
            // Apply the search
            if (!String.IsNullOrEmpty(search))
            {
                string[] searchElements = search.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string searchElement in searchElements)
                {
                    string element = searchElement;
                    queryable = queryable.Where(c => c.CustomerName.Contains(element));
                }
            }

            // Add the sorting
            if (sort != null)
                queryable = queryable.ApplySorting(sort);
            else
                queryable = queryable.OrderBy(c => c.InvoiceNumber);

            // Get the total number of records
            int totalItemCount = queryable.Count();

            // Retrieve the customers for the specified page customer
            var invoice = queryable
                .Skip(skip)
                .Take(pageSize)
                .ToList();
            List<InvoiceViewModel> viewinvoice = new List<InvoiceViewModel>();
            //export to viewmodel
            foreach (var item in invoice)
            {
                InvoiceViewModel viewitem = new InvoiceViewModel();

                viewitem.OID = item.OID;
                viewitem.InvoiceNumber = item.InvoiceNumber;
                viewitem.InvoiceDate = item.InvoiceDate;
                viewitem.CustomerDunsID = item.CustomerDunsID;
                viewitem.CustomerName = item.CustomerName;
                viewitem.VendorDUNSID = item.VendorDUNSID;
                viewitem.VendorName = item.VendorName;
                viewitem.TotalAmount = item.TotalAmount;
                viewitem.InvoiceFileFullPath = item.InvoiceFileFullPath;
                viewitem.InvoceNumberScore = item.InvoceNumberScore;
                viewitem.InvoiceDateScore = item.InvoiceDateScore;
                viewitem.CustomerDunsIDScore = item.CustomerDunsIDScore;
                viewitem.CustomerNameScore = item.CustomerNameScore;
                viewitem.VendoreDunsIDScore = item.VendoreDunsIDScore;
                viewitem.VendorNameScore = item.VendorNameScore;
                viewitem.TotalAmountScore = item.TotalAmountScore;
                viewitem.WeightedAverageScore = item.WeightedAverageScore;
                //Accuracy
                viewitem.ScoreWeight = (item.Accuracy == null) ? 0 : item.Accuracy.ScoreWeight;
                viewitem.AccuracyRate = (item.Accuracy == null) ? 0 : item.Accuracy.AccuracyRate;

                viewinvoice.Add(viewitem);

            }
            // Return the paged results
            return new PagedResult<InvoiceViewModel>(viewinvoice, pageNo, pageSize, totalItemCount);
        }

        [HttpDelete]
        public void DeletItem(string oID)
        {
            int idinvoice = Convert.ToInt32(oID);
            var invoiceToRemove = dbContext.Invoice.SingleOrDefault(x => x.OID == idinvoice);
            dbContext.Invoice.Remove(invoiceToRemove);
            dbContext.SaveChanges();

        }

        // POST api/<controller>
        [HttpPost]
        public HttpResponseMessage UploadPDFFile()
        {
            var request = HttpContext.Current.Request;
            HttpResponseMessage result = null;
            string invoiceoid = request.Form.GetValues(0).GetValue(0).ToString();

            int intoid = Convert.ToInt16(invoiceoid);
            Invoice invoiceToAdd = dbContext.Invoice.SingleOrDefault(x => x.OID == intoid);

            if (request.Files.Count == 0)
            {
                result = Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            if (request.Files.Count != 0)
            {
                var postedFile = request.Files[0];
                var myUniqueFileName = string.Format(@"{0}.pdf", Guid.NewGuid());
                string path = System.Web.Hosting.HostingEnvironment.MapPath("~/PDF_File/" + myUniqueFileName);
                postedFile.SaveAs(path);

                if (invoiceToAdd.InvoiceFileFullPath != null)
                {
                    System.IO.File.Delete(invoiceToAdd.InvoiceFileFullPath);
                }

                invoiceToAdd.InvoiceFileFullPath = path;
                dbContext.SaveChanges();
                result = Request.CreateResponse(HttpStatusCode.Created);
            }

            return result;

        }


        [HttpPost]
        public HttpResponseMessage UploadExcelFile()
        {
            var request = HttpContext.Current.Request;
            HttpResponseMessage result = null;

            if (request.Files.Count == 0)
            {
                result = Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
            {
                var excelFile = request.Files[0];
                if (excelFile.FileName.EndsWith("xls") || (excelFile.FileName.EndsWith("xlsx")))
                {
                    string path = System.Web.Hosting.HostingEnvironment.MapPath("~/PDF_File/" + excelFile.FileName);

                    excelFile.SaveAs(path);

                    Excel.Application application = new Excel.Application();
                    Excel.Workbook workbook = application.Workbooks.Open(path);
                    Excel.Worksheet worksheet = workbook.ActiveSheet;
                    Excel.Range range = worksheet.UsedRange;

                    List<Invoice> listinvoic = new List<Invoice>();

                    for (int row = 2; row <= range.Rows.Count; row++)
                    {
                        try
                        {

                            string[] formats = { "MM/dd/yyyy", "mm.dd.yyyy" };
                            Invoice invoice = new Invoice();

                            invoice.InvoiceNumber = Int32.Parse(((Excel.Range)range.Cells[row, 1]).Text);

                            invoice.InvoiceDate = DateTime.ParseExact((((Excel.Range)range.Cells[row, 2]).Text), formats,
                                                new System.Globalization.CultureInfo("en-US"),
                                                System.Globalization.DateTimeStyles.None);
                            invoice.CustomerDunsID = Int32.Parse(((Excel.Range)range.Cells[row, 3]).Text);
                            invoice.CustomerName = ((Excel.Range)range.Cells[row, 4]).Text;
                            invoice.VendorDUNSID = Int32.Parse(((Excel.Range)range.Cells[row, 5]).Text);
                            invoice.VendorName = ((Excel.Range)range.Cells[row, 6]).Text;
                            invoice.TotalAmount = Convert.ToDouble(((Excel.Range)range.Cells[row, 7]).Text);

                            dbContext.Invoice.Add(invoice);
                            dbContext.SaveChanges();
                            Accuracy addAcurecy = new Accuracy();
                            addAcurecy.OID = invoice.OID;
                            dbContext.Accuracy.Add(addAcurecy);
                            dbContext.SaveChanges();

                        }
                        catch (Exception)
                        {

                            throw;
                        }



                    }
                    application.Workbooks.Close();
                    System.IO.File.Delete(path);

                }


            }






            //string path = System.Web.Hosting.HostingEnvironment.MapPath("~/PDF_File/" + postedFile.FileName);
            //postedFile.SaveAs(path);

            result = Request.CreateResponse(HttpStatusCode.Created);
            return result;
        }

        [HttpGet]
        public HttpResponseMessage GetCSVFile()
        {
            HttpResponseMessage result = null;
            var sb = new StringBuilder();
            sb.Append("OID,InvoiceNumber,InvoiceDate,CustomerDunsID,CustomerName,VendorDUNSID,VendorName,TotalAmount,InvoiceFileFullPath,InvoceNumberScore,InvoiceDateScore,CustomerDunsIDScore,CustomerNameScore,VendoreDunsIDScore,VendorNameScore,TotalAmountScore,WeightedAverageScore,AccuracyRate,ScoreWeight\r\n");

            var invoiceList = dbContext.Invoice.ToList();

            foreach (Invoice invoice in invoiceList)
            {
                sb.AppendFormat("=\"{0}\",", invoice.OID);
                sb.AppendFormat("=\"{0}\",", invoice.InvoiceNumber);
                sb.AppendFormat("=\"{0}\",", invoice.InvoiceDate.ToString());
                sb.AppendFormat("=\"{0}\",", invoice.CustomerDunsID);
                sb.AppendFormat("=\"{0}\",", invoice.CustomerName);
                sb.AppendFormat("=\"{0}\",", invoice.VendorDUNSID);
                sb.AppendFormat("=\"{0}\",", invoice.VendorName);
                sb.AppendFormat("=\"{0}\",", invoice.TotalAmount);
                sb.AppendFormat("=\"{0}\",", invoice.InvoiceFileFullPath);
                sb.AppendFormat("=\"{0}\",", invoice.InvoceNumberScore);
                sb.AppendFormat("=\"{0}\",", invoice.InvoiceDateScore);
                sb.AppendFormat("=\"{0}\",", invoice.CustomerDunsIDScore);
                sb.AppendFormat("=\"{0}\",", invoice.CustomerNameScore);
                sb.AppendFormat("=\"{0}\",", invoice.VendoreDunsIDScore);
                sb.AppendFormat("=\"{0}\",", invoice.VendorNameScore);
                sb.AppendFormat("=\"{0}\",", invoice.TotalAmountScore);

                Accuracy acr = new Accuracy();
                acr = invoice.Accuracy;


                if (acr != null)
                {
                    sb.AppendFormat("=\"{0}\",", invoice.WeightedAverageScore);
                    sb.AppendFormat("=\"{0}\",", invoice.Accuracy.AccuracyRate);
                    sb.AppendFormat("=\"{0}\",", invoice.Accuracy.ScoreWeight);
                    sb.AppendFormat("=\"{0}\"\r\n", invoice.Accuracy.ScoreWeight); //no comma for the last item, but a new line
                }
                if (acr == null)
                {
                    sb.AppendFormat("=\"{0}\"\r\n", invoice.WeightedAverageScore); //no comma for the last item, but a new line
             
                }

            }
            result = new HttpResponseMessage(HttpStatusCode.OK);

            result.Content = new StringContent(sb.ToString());
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment"); //attachment will force download
            result.Content.Headers.ContentDisposition.FileName = "RecordExport.csv";

            return result;
        }
    }
}