using System;
using System.Collections.Generic;
using ConnectApp.Models.Model;

namespace ConnectApp.Models.State {
    [Serializable]
    public class ArticleState {
        public bool articlesLoading { get; set; }
        public bool articleDetailLoading { get; set; }
        public List<string> articleList { get; set; }
        public bool hottestHasMore { get; set; }
        public Dictionary<string, Article> articleDict { get; set; }
        public List<Article> articleHistory { get; set; }
        public List<string> blockArticleList { get; set; }
    }
}