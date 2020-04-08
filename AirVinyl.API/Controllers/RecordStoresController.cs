using System.Linq;
using System.Web.Http;
using AirVinyl.API.Helpers;
using AirVinyl.DataAccessLayer;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;

namespace AirVinyl.API.Controllers
{
	public class RecordStoresController : ODataController
	{
		private AirVinylDbContext _ctx = new AirVinylDbContext();

		[EnableQuery]
		public IHttpActionResult Get()
		{
			return Ok(_ctx.RecordStores);
		}

		[EnableQuery]
		public IHttpActionResult Get([FromODataUri] int key)
		{

			var recordStores = _ctx.RecordStores.Where(p => p.RecordStoreId == key);
			if (!recordStores.Any())
			{
				return NotFound();
			}

			return Ok(SingleResult.Create(recordStores));
		}

		[HttpGet]
		[ODataRoute("RecordStores({key})/Tags")]
		public IHttpActionResult GetRecordsStoreTagsProperty([FromODataUri]int key)
		{
			var current = _ctx.RecordStores.Where(r => r.RecordStoreId == key);
			if (current == null)
				return NotFound();

			var collectionPropertyToGet = Url.Request.RequestUri.Segments.Last();
			var collectionPropertyValue = current.GetValue(collectionPropertyToGet);

			return this.CreateOkHttpActionResult(collectionPropertyValue);
		}

		protected override void Dispose(bool disposing)
		{
			_ctx.Dispose();
			base.Dispose(disposing);
		}
	}
}