using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using log4net;
using System.Data.SqlClient;
using Dapper;
using System.IO;

namespace ArnavutkoyApiEntegrasyon
{
    class Program
    {
        static ILog log = LogManager.GetLogger("Arnavutkoy RestApi");
     
        static readonly RestClient client = new RestClient("http://172.22.1.145:7080");

        static void Main(string[] args)
        {         
          send_Incoming();
          send_goout();            
        }   
      
      

        static List<ZiyaretciTransaction> GetVisitors(string getQuery, string updateQuery)
        {
            List<ZiyaretciTransaction> ziyaretciler;
            using (var mycon = new SqlConnection(ConfigurationManager.ConnectionStrings["PdksMain"].ConnectionString))
            {
                ziyaretciler = mycon.Query<ZiyaretciTransaction>(getQuery).ToList();
                mycon.Execute(updateQuery);
            }
            return ziyaretciler;
        }

        static void BadRequestUpdate(int transactionId)
        {
            using (var mycon = new SqlConnection(ConfigurationManager.ConnectionStrings["PdksMain"].ConnectionString))
            {
                mycon.Execute(SqlQueries.BadRequestUpdate, new { TransactionId = transactionId });
            }
        }


        static void SuccessRequestUpdate(int visitId,int transactionId)
        {
            using (var mycon = new SqlConnection(ConfigurationManager.ConnectionStrings["PdksMain"].ConnectionString))
            {
                mycon.Execute(SqlQueries.SuccessRequestUpdate, new { VisitId = visitId, TransactionId = transactionId });
            }
        }


        static void send_Incoming()
        {
            client.AddDefaultHeader("Content-type", "application/x-www-form-urlencoded");

            try
            {
                var result = GetVisitors(SqlQueries.GetAllIncoming, SqlQueries.IncomingUpdate);

                var request = new RestRequest("vst/visit/set-in-gate", Method.POST);
                foreach (var item in result)
                {
                    request.AddParameter("cardNo", item.OzelAlan1);
                    request.AddParameter("gateName", item.Aciklama);
                    request.AddParameter("gateCode", item.TerminalId);
                    request.AddParameter("datetime", item.Tarih.ToString("yyyy-MM-dd HH:mm:ss"));

                    var response = client.Post(request);
                    //response.StatusCode = HttpStatusCode.OK;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var donendegerim = JsonConvert.DeserializeObject<ResponseInComingSuccess>(response.Content);
                        //donendegerim.visitId = 123;
                        SuccessRequestUpdate(donendegerim.data,item.TransactionId);
                        log.Info(DateTime.Now + " " + item.OzelAlan1 + "kart nolu ziyatetçinin girişi betik servisine başarıyla gönderildi.Dönen VisitId: " + donendegerim.data);
                    }
                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        var donendegerim = JsonConvert.DeserializeObject<ResponseBadRequest>(response.Content);
                        log.Error(DateTime.Now +" "+  item.OzelAlan1 + " nolu ziyatetçinin girişi " + donendegerim.exceptionMessage + " hatasından dolayı betik servisine yazılamadı");
                        BadRequestUpdate(item.TransactionId);
                    }
                    else
                    {
                        log.Error(DateTime.Now + " " + response.ErrorMessage + "hatasından dolayı servise yazılamadı");
                        BadRequestUpdate(item.TransactionId);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        static void send_goout()
        {
            client.AddDefaultHeader("Content-type", "application/x-www-form-urlencoded");

            try
            {
                var result = GetVisitors(SqlQueries.GetAllOutgoing, SqlQueries.OutgoingUpdate);

                var request = new RestRequest("/vst/visit/set-out-gate-and-end-visit", Method.POST);

                foreach (var item in result)
                {
                    int VisitId;
                    using (var mycon = new SqlConnection(ConfigurationManager.ConnectionStrings["PdksMain"].ConnectionString))
                    {
                        VisitId = mycon.ExecuteScalar<int>(SqlQueries.GetVisitId, new { PersonelId = item.PersonelId });
                    }                  
                    
                    request.AddParameter("visitId", VisitId);                    
                    request.AddParameter("gateName", item.Aciklama);
                    request.AddParameter("gateCode", item.TerminalId);
                    request.AddParameter("datetime", item.Tarih.ToString("yyyy-MM-dd HH:mm:ss"));
                    var response = client.Post(request);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var donendegerim = JsonConvert.DeserializeObject<ResponseGoOutSuccess>(response.Content);
                        log.Info(DateTime.Now + " " + item.OzelAlan1 + " nolu ziyatetçinin çıkışı betik servisine başarıyla gönderildi");
                    }
                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        var donendegerim = JsonConvert.DeserializeObject<ResponseBadRequest>(response.Content);
                        log.Error(DateTime.Now + " " + item.OzelAlan1 + " nolu ziyatetçinin çıkışı " + donendegerim.exceptionMessage + " hatasından dolayı betik servise yazılamadı");
                        BadRequestUpdate(item.TransactionId);
                    }
                    else
                    {
                        log.Error(DateTime.Now + " " + response.ErrorMessage + "hatasından dolayı servise yazılamadı");
                        BadRequestUpdate(item.TransactionId);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}

