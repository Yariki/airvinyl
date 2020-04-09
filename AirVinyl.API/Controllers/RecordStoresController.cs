using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AirVinyl.API.Helpers;
using AirVinyl.DataAccessLayer;
using AirVinyl.Model;
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


		[HttpGet]
		[ODataRoute("RecordStores({key})/AirVinyl.Functions.IsHighRated(minimumRating={minimumRating})")]
		public bool IsHighRated<T>([FromODataUri]int key, int minimumRating)
		{
			var recordStore = _ctx.RecordStores.FirstOrDefault(p => p.RecordStoreId == key
			                                                        && p.Ratings.Any()
			                                                        && (p.Ratings.Sum(r => r.Value) /
			                                                            p.Ratings.Count) >= minimumRating);
			return (recordStore != null);
		}

		[HttpGet]
		[ODataRoute("RecordStores/AirVinyl.Functions.AreRatedBy(personIds={personIds})")]
		public IHttpActionResult AreRatedBy([FromODataUri] IEnumerable<int> personIds)
		{
			var recordStores = _ctx.RecordStores
			                       .Where(p => p.Ratings.Any(r => personIds.Contains(r.RatedBy.PersonId)));

			return this.CreateOkHttpActionResult(recordStores);
		}

		[HttpGet]
		[ODataRoute("GetHighRatedRecordStores(minimumRating={minimumRating})")]
		public IHttpActionResult GetHighRatedRecordStores([FromODataUri] int minimumRating)
		{
			var recordStores = _ctx.RecordStores.Where(p => p.Ratings.Any()
			                                               && (p.Ratings.Sum(r => r.Value) /
			                                                   p.Ratings.Count) >= minimumRating);

			return this.CreateOkHttpActionResult(recordStores);
		}

		[HttpPost]
		[ODataRoute("RecordStores({key})/AirVinyl.Actions.Rate")]
		public IHttpActionResult Rate([FromODataUri]int key, ODataActionParameters parameters)
		{
			var recordStore = _ctx.RecordStores.FirstOrDefault(p => p.RecordStoreId == key);
			if(recordStore == null)
			{
				return NotFound();
			}

			int rating;
			int personId;
			object outputFromDictionary;

			if(!parameters.TryGetValue("rating",out outputFromDictionary))
			{
				return NotFound();
			}
			if(!int.TryParse(outputFromDictionary.ToString(), out rating))
			{
				return NotFound();
			}

			if(!parameters.TryGetValue("personId", out outputFromDictionary))
			{
				return NotFound();
			}
			if(!int.TryParse(outputFromDictionary.ToString(), out personId))
			{
				return NotFound();
			}

			var person = _ctx.People.FirstOrDefault(p => p.PersonId == personId);
			if(person == null)
			{
				return NotFound();
			}

			recordStore.Ratings.Add(new Rating(){RatedBy = person, Value = rating});
			if(_ctx.SaveChanges() > -1)
			{
				return this.CreateOkHttpActionResult(true);
			}
			else
			{
				return this.CreateOkHttpActionResult(false);
			}
		}

		protected override void Dispose(bool disposing)
		{
			_ctx.Dispose();
			base.Dispose(disposing);
		}
	}
}