using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Td = Telegram.Td;
using TdApi = Telegram.Td.Api;

namespace TdExample
{
    class Raspa
    {
        private Td.Client _client;
        public Raspa(Td.Client client)
        {
            _client = client;
        }

        public void Test()
        {
            long testgroupid = -1001836947673;
            long ikigaichannelid = -1001209790850;
            long testinvitoid = -1001751133994;
            long lorisid = 207118965;
            long mioid = 5908863946;
            long ikigaitestid = 5897547742;
            long ariannaid = 5263402001;
            _client.Send(new TdApi.AddChatMember(ikigaichannelid, ariannaid, 0), null);
        }
    }
}
