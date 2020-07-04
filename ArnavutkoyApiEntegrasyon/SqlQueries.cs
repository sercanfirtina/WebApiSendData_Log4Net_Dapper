using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArnavutkoyApiEntegrasyon
{
    static class SqlQueries
    {

        public const string GetAllIncoming = "select pt.TransactionId, p.personelId, OzelAlan1,t.Aciklama,T.TerminalId,pt.Tarih,pt.Iletim from PersonelTransaction pt left join Personel p on pt.PersonelId = p.PersonelId left join Terminal t on pt.TerminalId = t.TerminalId where pt.Yon = 1 and p.GrupId = 9 and Iletim = 0 order by Tarih desc";
        public const string GetAllOutgoing = "select pt.TransactionId, p.personelId, OzelAlan1,t.Aciklama,T.TerminalId,pt.Tarih,pt.Iletim from PersonelTransaction pt left join Personel p on pt.PersonelId = p.PersonelId left join Terminal t on pt.TerminalId = t.TerminalId where pt.Yon = 2 and p.GrupId = 9 and Iletim = 0 order by Tarih desc";
        public const string IncomingUpdate = "update PersonelTransaction set Iletim=1 where Iletim=0 and Yon=1";
        public const string OutgoingUpdate = "update PersonelTransaction set Iletim=1 where Iletim=0 and Yon=2";
        public const string BadRequestUpdate = "update PersonelTransaction set Iletim=0 where TransactionId=@TransactionId";
        public const string SuccessRequestUpdate = "update PersonelTransaction set VisitId=@visitId where TransactionId=@TransactionId";
        public const string GetVisitId = "select Top 1 VisitId from PersonelTransaction where PersonelId=@PersonelId and Yon = 1 Order by Tarih desc";
    }
}
