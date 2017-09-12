using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PodcastsSyndicate.Dal;
using PodcastsSyndicate.Models;

namespace PodcastsSyndicate.Controllers
{
    public class PodcastController : BaseController
    {
        [HttpPut("/podcast/{podcastId}")]
        public async Task<IActionResult> CreateOrUpdate(string podcastId, [FromBody] Podcast podcast)
        {
            if(!User.IsDeveloper)
                return Forbid("Bearer");

            if(podcastId != podcast.Id)
                return this.BadRequest("The podcast Id does not match the endpoint Id.");
            
            await Db.Podcasts.Document(podcastId).DeleteAsync();
            await Db.Podcasts.AddAsync(podcast);

            return Created($"/podcast/{podcastId}", await Db.Podcasts.Document(podcastId).ReadAsync());
        }

        [HttpGet("/podcast/{podcastId}/rss")]
        [HttpHead("/podcast/{podcastId}/rss")]
        [Produces("application/xml")]
        public async Task<IActionResult> GetRss(string podcastId)
        {
            var podcast = await Db.Podcasts.Document(podcastId).ReadAsync();

            // Document declaration.

            var doc = new XDocument();
            doc.Declaration = new XDeclaration("1.0", "utf-8", null);
            XNamespace itunes = "http://www.itunes.com/dtds/podcast-1.0.dtd";
            XNamespace atom = "http://www.w3.org/2005/Atom";

            // RSS declaration.

            var rss = new XElement("rss"); doc.Add(rss);
            rss.SetAttributeValue(XNamespace.Xmlns + "itunes", itunes);
            rss.SetAttributeValue(XNamespace.Xmlns + "atom", atom);
            rss.SetAttributeValue("version", "2.0");

            // Channel declaration.
            
            var channel = new XElement("channel"); rss.Add(channel);
            channel.Add(new XElement("title", podcast.Title));
            channel.Add(new XElement("link", $"http://podcastssyndicate.com/podcast/{podcast.Id}"));
            channel.Add(new XElement("description", podcast.Description));
            channel.Add(new XElement("language", podcast.Language));

            // iTunes channel declarations.

            channel.Add(new XElement(itunes + "author", podcast.Author));
            channel.Add(new XElement(itunes + "subtitle", podcast.Subtitle));
            channel.Add(new XElement(itunes + "summary", podcast.Description));
            var image = new XElement(itunes + "image"); image.SetAttributeValue("href", podcast.Image); channel.Add(image);
            channel.Add(new XElement(itunes + "explicit", podcast.Explicit ? "yes" : "no"));

            // iTunes owner declaration.

            var owner = new XElement(itunes + "owner"); channel.Add(owner);
            owner.Add(new XElement(itunes + "name", podcast.Author));
            owner.Add(new XElement(itunes + "email", podcast.AuthorEmail));

            // ATOM self link.

            //var atomLink = new XElement(atom + "link"); atomLink.SetAttributeValue("href", Request.Host + Request.Path); atomLink.SetAttributeValue("rel", "self"); atomLink.SetAttributeValue("type", "application/rss+xml"); channel.Add(atomLink);
            
            // Category declarations.

            foreach(var category in podcast.Categories)
            {
                var cat = new XElement(itunes + "category"); cat.SetAttributeValue("text", category); channel.Add(cat);
            }

            // Item declarations.

            foreach(var episode in podcast.Episodes.OrderByDescending(e => e.PublishDate))
            {
                // Get content size.
                var headRequest = new HttpRequestMessage(HttpMethod.Head, episode.Link);
                var response = await new HttpClient().SendAsync(headRequest);
                var contentLength = response.Content.Headers.GetValues("Content-Length").First();
                var contentType = response.Content.Headers.GetValues("Content-Type").First();

                // Item declaration.

                var item = new XElement("item"); channel.Add(item);
                item.Add(new XElement("title", episode.Title));
                item.Add(new XElement("link", episode.Link));
                var enclosure = new XElement("enclosure"); enclosure.SetAttributeValue("url", episode.Link); enclosure.SetAttributeValue("length", contentLength); enclosure.SetAttributeValue("type", contentType); item.Add(enclosure);
                item.Add(new XElement("guid", episode.Link));
                item.Add(new XElement("pubDate", episode.PublishDate.ToString("r")));
                item.Add(new XElement("description", podcast.Description));

                // iTunes item declarations.

                item.Add(new XElement(itunes + "author", podcast.Author));
                item.Add(new XElement(itunes + "subtitle", episode.Subtitle));
                item.Add(new XElement(itunes + "summary", episode.Description));
                item.Add(new XElement(itunes + "keywords", string.Join(", ", episode.Tags)));
                var itemImage = new XElement(itunes + "image"); itemImage.SetAttributeValue("href", episode.Image); item.Add(itemImage);
                item.Add(new XElement(itunes + "duration", $"{episode.Duration / 3600:00}:{(episode.Duration % 3600) / 60:00}:{(episode.Duration % 3600) % 60:00}"));
            }

            return Ok(doc.Root);
        }
    }
}
