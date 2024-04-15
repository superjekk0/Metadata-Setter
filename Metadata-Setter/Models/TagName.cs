using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metadata_Setter.Models
{
    public enum TagName
    {
        [Description("Album")]
        Album,
        [Description("Album Artists")]
        AlbumArtists,
        [Description("Amazon ID")]
        AmazonID,
        [Description("Artists")]
        Artists,
        [Description("Beats per Minute")]
        BeatsPerMinute,
        [Description("Comment")]
        Comment,
        [Description("Composers")]
        Composers,
        [Description("Composers Sort")]
        ComposersSort,
        [Description("Conductor")]
        Conductor,
        [Description("Copyright")]
        Copyright,
        [Description("Description")]
        Description,
        [Description("Disc")]
        Disc,
        [Description("Disc Count")]
        DiscCount,
        [Description("Genres")]
        Genre,
        [Description("Grouping")]
        Grouping,
        [Description("Initial Key")]
        InitialKey,
        [Description("ISRC")]
        ISRC,
        [Description("Lyrics")]
        Lyrics,
        [Description("Performers")]
        Performers,
        [Description("Performers Sort")]
        PerformersSort,
        [Description("Performers Role")]
        PerformersRole,
        //[Description("Pictures")]
        //Pictures,
        [Description("Publisher")]
        Publisher,
        [Description("Remixed By")]
        RemixedBy,
        [Description("Subtitle")]
        Subtitle,
        [Description("Title")]
        Title,
        [Description("Title Sort")]
        TitleSort,
        [Description("Track")]
        Track,
        [Description("Track Count")]
        TrackCount,
        [Description("Year")]
        Year,
    }
}
