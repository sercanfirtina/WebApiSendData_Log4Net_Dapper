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


namespace ArnavutkoyApiEntegrasyon
{
    public class ZiyaretciTransaction
    {
        public int TransactionId { get; set; }
        public string OzelAlan1 { get; set; }
        public int PersonelId { get; set; }
        public string Aciklama { get; set; }
        public int TerminalId { get; set; }
        public DateTime Tarih { get; set; }
        public int VisitId { get; set; }
    }
}
