using System.CodeDom;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web.Http;
using AirVinyl.DataAccessLayer;
using AirVinyl.Model;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;

namespace AirVinyl.API.Controllers
{
	public class AirVinylController : ODataController
	{
		private AirVinylDbContext _ctx = new AirVinylDbContext();


		[HttpGet]
		[ODataRoute("VinylRecords")]
		public IHttpActionResult GetAllVinylsRecords()
		{
			return Ok(_ctx.VinylRecords);
		}

		[HttpGet]
		[ODataRoute("VinylRecords({key})")]
		public IHttpActionResult Get(int key)
		{
			var record = _ctx.VinylRecords.FirstOrDefault(r => r.VinylRecordId == key);
			if(record == null)
			{
				return NotFound();
			}

			return Ok(record);
		}

		protected override void Dispose(bool disposing)
		{
			_ctx.Dispose();
			
			base.Dispose(disposing);
		}
	}
}