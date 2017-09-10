using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
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

            await Db.Podcasts.AddAsync(podcast);

            return Created($"/podcast/{podcastId}", await Db.Podcasts.Document(podcastId).ReadAsync());
        }

        [HttpGet("/podcast/{podcastId}/rss")]
        [Produces("application/xml")]
        public async Task<IActionResult> GetRss(string podcastId)
        {
            var podcast = await Db.Podcasts.Document(podcastId).ReadAsync();

            var doc = new XDocument();
            doc.Declaration = new XDeclaration("1.0", "utf-8", null);
            XNamespace itunes = "http://www.itunes.com/dtds/podcast-1.0.dtd";
            XNamespace atom = "http://www.w3.org/2005/Atom";

            var rss = new XElement("rss"); doc.Add(rss);
            rss.SetAttributeValue(XNamespace.Xmlns + "itunes", itunes);
            rss.SetAttributeValue(XNamespace.Xmlns + "atom", atom);
            rss.SetAttributeValue("version", "2.0");
            
            var channel = new XElement("channel"); rss.Add(channel);
            channel.Add(new XElement("title", podcast.Title));
            channel.Add(new XElement("link", $"http://podcastssyndicate.com/podcast/{podcast.Id}"));
            channel.Add(new XElement("description", podcast.Description));
            channel.Add(new XElement("language", podcast.Language));

            channel.Add(new XElement(itunes + "author", podcast.Author));
            channel.Add(new XElement(itunes + "subtitle", podcast.Subtitle));
            channel.Add(new XElement(itunes + "summary", podcast.Description));
            var image = new XElement(itunes + "image"); image.SetAttributeValue("href", podcast.Image); channel.Add(image);
            channel.Add(new XElement(itunes + "explicit", podcast.Explicit ? "yes" : "no"));

            var owner = new XElement(itunes + "owner"); channel.Add(owner);
            owner.Add(new XElement(itunes + "name", podcast.Author));
            owner.Add(new XElement(itunes + "email", podcast.AuthorEmail));

            var atomLink = new XElement(atom + "link"); atomLink.SetAttributeValue("href", $"http://rss.{podcastId}.podcastssyndicate.com"); atomLink.SetAttributeValue("rel", "self"); atomLink.SetAttributeValue("type", "application/rss+xml"); channel.Add(atomLink);
            
            foreach(var category in podcast.Categories)
            {
                var cat = new XElement(itunes + "category"); cat.SetAttributeValue("text", category); channel.Add(cat);
            }

            foreach(var episode in podcast.Episodes.OrderByDescending(e => e.PublishDate))
            {
                var item = new XElement("item"); channel.Add(item);

                item.Add(new XElement("title", episode.Title));
                item.Add(new XElement("link", episode.Link));
                var enclosure = new XElement("enclosure"); enclosure.SetAttributeValue("url", episode.Link); enclosure.SetAttributeValue("length", 0); enclosure.SetAttributeValue("type", "audio/mpeg"); item.Add(enclosure);
                item.Add(new XElement("guid", $"http://podcastssyndicate.com/podcast/{podcast.Id}/episode/{episode.Id}"));
                item.Add(new XElement("pubDate", episode.PublishDate.ToString("r")));

                item.Add(new XElement(itunes + "author", podcast.Author));
                item.Add(new XElement(itunes + "subtitle", episode.Subtitle));
                item.Add(new XElement(itunes + "summary", episode.Description));
                item.Add(new XElement(itunes + "duration", episode.Duration));
                item.Add(new XElement(itunes + "keywords", string.Join(", ", episode.Tags)));
                var itemImage = new XElement(itunes + "image"); itemImage.SetAttributeValue("href", episode.Image); item.Add(itemImage);
            }

            return Ok(doc.Root);
        }
    }
}
