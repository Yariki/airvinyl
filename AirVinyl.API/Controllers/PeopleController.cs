using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using AirVinyl.API.Helpers;
using AirVinyl.DataAccessLayer;
using AirVinyl.Model;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace AirVinyl.API.Controllers
{
	public class PeopleController : ODataController
	{
		private AirVinylDbContext _ctx = new AirVinylDbContext();

		[EnableQuery]
		public IHttpActionResult Get()
		{
			return Ok(_ctx.People);
		}

		[EnableQuery]
		public IHttpActionResult Get([FromODataUri]int key)
		{
			//var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);

			//if (person == null)
			//{
			//	return NotFound();
			//}

			//return Ok(person);

			var person = _ctx.People.Where(p => p.PersonId == key);
			if(!person.Any())
			{
				return NotFound();
			}

			return Ok(SingleResult.Create(person));
		}

		[HttpGet]
		[ODataRoute("People({key})/Email")]
		[ODataRoute("People({key})/FirstName")]
		[ODataRoute("People({key})/LastName")]
		[ODataRoute("People({key})/DateOfBirth")]
		[ODataRoute("People({key})/Gender")]
		public IHttpActionResult GetPersonProperty([FromODataUri] int key)
		{
			var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
			if (person == null)
			{
				return NotFound();
			}

			var propertyName = Url.Request.RequestUri.Segments.Last();
			if (!person.HasProperty(propertyName))
			{
				return NotFound();
			}

			var propValue = person.GetValue(propertyName);

			if (propValue == null)
			{
				return StatusCode(HttpStatusCode.NoContent);
			}

			return this.CreateOkHttpActionResult(propValue);
		}

		[HttpGet]
		[EnableQuery]
		[ODataRoute("People({key})/VinylRecords")]
		public IHttpActionResult GetVinylRecordsForPerson([FromODataUri] int key)
		{
			var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);

			if (person == null)
				return NotFound();

			return Ok(_ctx.VinylRecords.Where(v => v.Person.PersonId == key));
		}


		[HttpGet]
		[EnableQuery]
		[ODataRoute("People({key})/VinylRecords({recordId})")]
		public IHttpActionResult GetVynilRecordForPerson([FromODataUri]int key, [FromODataUri]int recordId)
		{
			var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
			if (person == null)
				return NotFound();

			var vinylRecords = _ctx.VinylRecords.Where(r => r.VinylRecordId == recordId && r.Person.PersonId == key);
			if (!vinylRecords.Any())
				return NotFound();

			return Ok(SingleResult.Create(vinylRecords));
		}

		#region [VinylRecords]


		[HttpPost]
		[ODataRoute("People({key})/VinylRecords")]
		public IHttpActionResult CreateVinylRecordForPerson([FromODataUri] int key, VinylRecord record)
		{
			if(!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
			if(person == null)
			{
				return NotFound();
			}

			record.Person = person;
			_ctx.VinylRecords.Add(record);
			_ctx.SaveChanges();

			return Created(record);
		}


		#endregion

		[EnableQuery]
		[HttpGet]
		[ODataRoute("People({key})/Friends")]
		//[ODataRoute("People({key})/VinylRecords")]
		public IHttpActionResult GetPersonCollectionProperty([FromODataUri] int key)
		{
			var segment = Url.Request.RequestUri.Segments.Last();
			var person = _ctx.People.Include(segment).FirstOrDefault(p => p.PersonId == key);

			if (person == null)
				return NotFound();

			var colValue = person.GetValue(segment);

			return this.CreateOkHttpActionResult(colValue);
		}
		
		[HttpGet]
		[ODataRoute("People({key})/Email/$value")]
		[ODataRoute("People({key})/FirstName/$value")]
		[ODataRoute("People({key})/LastName/$value")]
		[ODataRoute("People({key})/DateOfBirth/$value")]
		[ODataRoute("People({key})/Gender/$value")]
		public IHttpActionResult GetPersonPropertyRawValue([FromODataUri] int key)
		{
			var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
			if (person == null)
			{
				return NotFound();
			}

			var propertyName = Url.Request.RequestUri
				.Segments[Url.Request.RequestUri.Segments.Length - 2].TrimEnd('/');
			if (!person.HasProperty(propertyName))
			{
				return NotFound();
			}

			var propValue = person.GetValue(propertyName);

			if (propValue == null)
			{
				return StatusCode(HttpStatusCode.NoContent);
			} 

			return this.CreateOkHttpActionResult(propValue.ToString());
		}

		public IHttpActionResult Post(Person person)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			_ctx.People.Add(person);
			_ctx.SaveChanges();

			return Created(person);
		}

		public IHttpActionResult Put([FromODataUri] int key, Person person)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var currentPerson = _ctx.People.FirstOrDefault(p => p.PersonId == key);
			if (currentPerson == null)
				return NotFound();

			person.PersonId = currentPerson.PersonId;
			_ctx.Entry(currentPerson).CurrentValues.SetValues(person);
			_ctx.SaveChanges();

			return StatusCode(HttpStatusCode.NoContent);
		}

		public IHttpActionResult Patch([FromODataUri] int key, Delta<Person> path)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var current = _ctx.People.FirstOrDefault(p => p.PersonId == key);
			if (current == null)
			{
				return NotFound();
			}
			
			path.Patch(current);
			_ctx.SaveChanges();

			return StatusCode(HttpStatusCode.NoContent);
		}
		

		public IHttpActionResult Delete([FromODataUri]int key)
		{
			var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);

			if(person == null)
			{
				return NotFound();
			}

			var peopleWithCurrentPersonAsFriend = _ctx.People.Include("Friends")
			                                          .Where(p => p
			                                                      .Friends.Select(f => f.PersonId).AsQueryable()
			                                                      .Contains(key));

			foreach (var person1 in peopleWithCurrentPersonAsFriend)
			{
				person1.Friends.Remove(person);
			}

			_ctx.SaveChanges();

			_ctx.People.Remove(person);
			_ctx.SaveChanges();

			return StatusCode(HttpStatusCode.NoContent);
		}


		[HttpPost]
		[ODataRoute("People({key})/Friends/$ref")]
		public IHttpActionResult CreateLinkToFriend([FromODataUri] int key, [FromBody] Uri link)
		{
			var current = _ctx.People.Include("Friends").FirstOrDefault(p => p.PersonId == key);
			if(current == null)
			{
				return NotFound();
			}

			int keyOfFriendToAdd = link.GetIntKey();
			if(current.Friends.Any(i => i.PersonId == keyOfFriendToAdd))
			{
				return BadRequest(
					$"The person with id {keyOfFriendToAdd} is already linked to the person with id {key}");
			}

			var friendToLinkTo = _ctx.People.FirstOrDefault(p => p.PersonId == keyOfFriendToAdd);
			if(friendToLinkTo == null)
			{
				return NotFound();
			}

			current.Friends.Add(friendToLinkTo);
			_ctx.SaveChanges();

			return StatusCode(HttpStatusCode.NoContent);

		}

		[HttpPut]
		[ODataRoute("People({key})/Friends({relatedKey})/$ref")]
		public IHttpActionResult UpdateLinkToFriend([FromODataUri] int key,[FromODataUri] int relatedKey, [FromBody] Uri link)
		{
			var current = _ctx.People.Include("Friends").FirstOrDefault(p => p.PersonId == key);
			if (current == null)
			{
				return NotFound();
			}

			var currentFriend = current.Friends.FirstOrDefault(p => p.PersonId == relatedKey);
			if(currentFriend == null)
			{
				return NotFound();
			}

			int keyOfFriendToAdd = link.GetIntKey();
			if (current.Friends.Any(i => i.PersonId == keyOfFriendToAdd))
			{
				return BadRequest(
					$"The person with id {keyOfFriendToAdd} is already linked to the person with id {key}");
			}

			var friendToLinkTo = _ctx.People.FirstOrDefault(p => p.PersonId == keyOfFriendToAdd);
			if (friendToLinkTo == null)
			{
				return NotFound();
			}

			current.Friends.Remove(currentFriend);
			current.Friends.Add(friendToLinkTo);

			_ctx.SaveChanges();

			return StatusCode(HttpStatusCode.NoContent);
		}

		[HttpDelete]
		[ODataRoute("People({key})/Friends({relatedKey})/$ref")]
		public IHttpActionResult DeleteLinkToFriend([FromODataUri] int key, [FromODataUri] int relatedKey)
		{
			var current = _ctx.People.Include("Friends").FirstOrDefault(p => p.PersonId == key);
			if (current == null)
			{
				return NotFound();
			}

			var friend = current.Friends.FirstOrDefault(f => f.PersonId == relatedKey);
			if(friend == null)
			{
				return NotFound();
			}

			current.Friends.Remove(friend);
			_ctx.SaveChanges();

			return StatusCode(HttpStatusCode.NoContent);
		}
		
		protected override void Dispose(bool disposing)
		{
			_ctx.Dispose();
			base.Dispose(disposing);
		}
	}
}