using System.Collections.Generic;

namespace VIToACS.Models
{
    public class Scene
    {
        [System.ComponentModel.DataAnnotations.Key]
        public string Id { get; set; }

        public double Start { get; set; }

        public double End { get; set; }

        public List<Shot> Shots { get; set; }

        public Video Video { get; set; }

        public List<Transcript> Transcript { get; set; }

        public List<Face> Faces { get; set; }

        public List<Emotion> Emotions { get; set; }

        public List<Sentiment> Sentiments { get; set; }

        public List<Label> Labels { get; set; }

        public List<AudioEffect> AudioEffects { get; set; }

        public Playlist Playlist { get; set; }

        /// <summary>
        /// Gets or sets the keywords
        /// </summary>
        public List<Keyword> Keywords { get; set; }
    }
}
