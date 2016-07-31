﻿using BaconBackend.Collectors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace BaconBackend.DataObjects
{
    /// <summary>
    /// A reddit post, either of a link or of text.
    /// A post has a score, which is the total number of up votes - total number of down votes.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class Post : BindableBase
    {
        ResourceContext resourceContext = new ResourceContext();
        ResourceMap resourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");

        /// <summary>
        /// The comment's unique ID. Prefixed with "t3_", this
        /// is the post's fullname.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// If the post is self-text, the post's subreddit, preceded by "self.".
        /// If the post is a link, the website domain the post is a link to.
        /// </summary>
        [JsonProperty(PropertyName = "domain")]
        public string Domain { get; set; }

        /// <summary>
        /// The subreddit this post occurs in.
        /// </summary>
        [JsonProperty(PropertyName = "subreddit")]
        public string Subreddit { get; set; }

        /// <summary>
        /// The post's self-text. If the post is a link, this is the empty string.
        /// </summary>
        [JsonProperty(PropertyName = "selftext")]
        public string Selftext { get; set; }

        /// <summary>
        /// The user who submitted this post.
        /// </summary>
        [JsonProperty(PropertyName = "author")]
        public string Author { get; set; }

        /// <summary>
        /// The comment's score: total up votes - total down votes.
        /// </summary>
        [JsonProperty(PropertyName = "score")]
        public int Score
        {
            get
            {
                return m_score;
            }
            set
            {
                this.SetProperty(ref this.m_score, value);
            }
        }
        [JsonIgnore]
        int m_score = 0;

        /// <summary>
        /// If this post is marked a only for ages 18 years or older.
        /// </summary>
        [JsonProperty(PropertyName = "over_18")]
        public bool IsOver18 { get; set; }

        /// <summary>
        /// If this post is stickied to the top of the subreddit's posts, when sorted by hotness.
        /// </summary>
        [JsonProperty(PropertyName = "stickied")]
        public bool IsStickied { get; set; }

        /// <summary>
        /// If this post is self-text, (instead of a link).
        /// </summary>
        [JsonProperty(PropertyName = "is_self")]
        public bool IsSelf { get; set; }

        /// <summary>
        /// The post's link, or the post's permalink if it is self-text.
        /// </summary>
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        /// <summary>
        /// The post's title.
        /// </summary>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Unix timestamp of the time this post was submitted.
        /// Or, the number of seconds that have passed since
        /// January 1, 1970 UTC until this post was submitted.
        /// </summary>
        [JsonProperty(PropertyName = "created_utc")]
        public double CreatedUtc { get; set; }

        /// <summary>
        /// The number of comments on this post, counting all replies to other comments.
        /// </summary>
        [JsonProperty(PropertyName = "num_comments")]
        public int NumComments { get; set; }

        /// <summary>
        /// A link to the preview image used when listing posts, "self" if the post is self-text,
        /// or the empty string if there is no preview image.
        /// </summary>
        [JsonProperty(PropertyName = "thumbnail")]
        public string Thumbnail { get; set; }

        /// <summary>
        /// A link to the post.
        /// </summary>
        [JsonProperty(PropertyName = "permalink")]
        public string Permalink { get; set; }

        /// <summary>
        /// The post's flair text.
        /// </summary>
        [JsonProperty(PropertyName = "link_flair_text")]
        public string LinkFlairText { get; set; }

        /// <summary>
        /// If the post is gilded or not.
        /// </summary>
        [JsonProperty(PropertyName = "gilded")]
        public bool Gilded { get; set; }

        /// <summary>
        /// true: the logged-in user upvoted the post.
        /// false: the logged-in user downvoted the post.
        /// null: the logged-in user has neither upvoted nor downvoted the post.
        /// </summary>
        [JsonProperty(PropertyName = "likes")]
        public bool? Likes
        {
            get
            {
                return m_likes;
            }
            set
            {
                if (this.SetProperty(ref this.m_likes, value))
                {
                    this.OnPropertyChanged(nameof(DownVoteColor));
                    this.OnPropertyChanged(nameof(UpVoteColor));
                }
            }
        }
        [JsonIgnore]
        bool? m_likes = null;

        /// <summary>
        /// Whether the logged in user has saved the post.
        /// </summary>
        [JsonProperty(PropertyName = "saved")]
        public bool IsSaved
        {
            get
            {
                return m_isSaved;
            }
            set
            {
                if(this.SetProperty(ref this.m_isSaved, value))
                {
                    OnPropertyChanged(nameof(IsSavedMenuText));
                }
            }
        }
        [JsonIgnore]
        bool m_isSaved;

        /// <summary>
        /// Whether the user hid the post from being listed normally.
        /// </summary>
        [JsonProperty(PropertyName = "hidden")]
        public bool IsHidden
        {
            get
            {
                return m_isHidden;
            }
            set
            {
                if (this.SetProperty(ref this.m_isHidden, value))
                {
                    OnPropertyChanged(nameof(IsHiddenMenuText));
                }
            }
        }
        [JsonIgnore]
        bool m_isHidden;

        /// <summary>
        /// Represents the current comment sort type for this post
        /// </summary>
        [JsonIgnore]
        public CommentSortTypes CommentSortType
        {
            get
            {
                return m_commentSortType;
            }
            set
            {
                if(SetProperty(ref m_commentSortType, value))
                {
                    OnPropertyChanged(nameof(CommentCurrentSortTypeString));
                }
            }
        }
        [JsonIgnore]
        CommentSortTypes m_commentSortType = CommentSortTypes.Best;

        /// <summary>
        /// Indicates if we have seeded this post with the defaults yet.
        /// </summary>
        [JsonIgnore]
        public bool HaveCommentDefaultsBeenSet = false;

        //
        // UI Vars
        //

        // Static Cache Colors
        private static Color s_colorWhite = Color.FromArgb(255, 255, 255, 255);
        private static Color s_colorGray = Color.FromArgb(255, 152, 152, 152);
        private static SolidColorBrush s_accentBrush = null;
        private static SolidColorBrush s_lightenedAccentBrush = null;
        private static SolidColorBrush s_darkenedAccentBrush = null;
        private static SolidColorBrush GetAccentBrush()
        {
            // Not thread safe, but that's ok
            if(s_accentBrush == null)
            {
                s_accentBrush = (SolidColorBrush)Application.Current.Resources["SystemControlBackgroundAccentBrush"];
            }
            return s_accentBrush;
        }

        private static SolidColorBrush GetLightenedAccentBrush()
        {
            // Not thread safe, but that's ok
            if (s_lightenedAccentBrush == null)
            {
                SolidColorBrush accentBrush = GetAccentBrush();
                Color accentColor = accentBrush.Color;
                accentColor.B = (byte)Math.Min(255, accentColor.B + 80);
                accentColor.R = (byte)Math.Min(255, accentColor.R + 80);
                accentColor.G = (byte)Math.Min(255, accentColor.G + 80);
                s_lightenedAccentBrush = new SolidColorBrush(accentColor);
            }
            return s_lightenedAccentBrush;
        }

        private static SolidColorBrush GetDarkenedAccentBrush()
        {
            // Not thread safe, but that's ok
            if (s_darkenedAccentBrush == null)
            {
                SolidColorBrush accentBrush = GetAccentBrush();
                Color accentColor = accentBrush.Color;
                accentColor.B = (byte)Math.Max(0, accentColor.B - 40);
                accentColor.R = (byte)Math.Max(0, accentColor.R - 40);
                accentColor.G = (byte)Math.Max(0, accentColor.G - 40);
                s_darkenedAccentBrush = new SolidColorBrush(accentColor);
            }
            return s_darkenedAccentBrush;
        }

        /// <summary>
        /// Used in the subreddit view for the sub text
        /// </summary>
        [JsonIgnore]
        public string SubTextLine1 { get; set; }

        /// <summary>
        /// Used in subreddit view for the 2nd line first color
        /// </summary>
        [JsonIgnore]
        public string SubTextLine2PartTwo { get; set; }

        /// <summary>
        /// Used in subreddit view for the 2nd line second color
        /// </summary>
        [JsonIgnore]
        public string SubTextLine2PartOne { get; set; }

        /// <summary>
        /// Used in flip view for the second line of text
        /// </summary>
        [JsonIgnore]
        public string FlipViewSecondary { get; set; }

        /// <summary>
        /// Used by subreddit view to show unread comment count
        /// </summary>
        [JsonIgnore]
        public string NewCommentText
        {
            get
            {
                return m_newCommentText;
            }
            set
            {
                if(SetProperty(ref m_newCommentText, value))
                {
                    OnPropertyChanged(nameof(NewCommentColor));
                    OnPropertyChanged(nameof(NewCommentMargin));
                }
            }
        }
        [JsonIgnore]
        string m_newCommentText = "";

        /// <summary>
        /// Used by the subreddit view to show the image grid
        /// </summary>
        [JsonIgnore]
        public Visibility ImageVisibility
        {
            get
            {
                return m_imageVisibility;
            }
            set
            {
                SetProperty(ref m_imageVisibility, value);
            }
        }
        [JsonIgnore]
        Visibility m_imageVisibility = Visibility.Collapsed;

        /// <summary>
        /// Used by subreddit view to hold the bit map
        /// </summary>
        [JsonIgnore]
        public BitmapImage Image
        {
            get
            {
                return m_image;
            }
            set
            {
                SetProperty(ref m_image, value);
            }
        }
        [JsonIgnore]
        BitmapImage m_image = null;

        /// <summary>
        /// Note! This is the color for the title but the UI really looks at the brush.
        /// Thus when the color update we want to fire the change on the brush.
        /// </summary>
        [JsonIgnore]
        public Color TitleTextColor
        {
            get
            {
                return m_titleTextColor;
            }
            set
            {
                m_titleTextColor = value;
                OnPropertyChanged(nameof(TitleTextBrush));
            }
        }
        [JsonIgnore]
        Color m_titleTextColor = s_colorWhite;

        /// <summary>
        /// Sets how many line the title can show max
        /// </summary>
        [JsonIgnore]
        public int TitleMaxLines
        {
            get
            {
                return m_titleMaxLines;
            }
            set
            {
                SetProperty(ref m_titleMaxLines, value);
            }
        }
        [JsonIgnore]
        int m_titleMaxLines = 2;

        /// <summary>
        /// Sets text for comment sort
        /// </summary>
        [JsonIgnore]
        public string CommentCurrentSortTypeString
        {
            get
            {
                switch(CommentSortType)
                {
                    default:
                    case CommentSortTypes.Best:
                        return "Best";
                    case CommentSortTypes.Controversial:
                        return "Controversial";
                    case CommentSortTypes.New:
                        return "New";
                    case CommentSortTypes.Old:
                        return "Old";
                    case CommentSortTypes.QA:
                        return "Q&A";
                    case CommentSortTypes.Top:
                        return "Top";
                }
            }
        }

        /// <summary>
        /// Sets text for a context menu item
        /// </summary>
        [JsonIgnore]
        public string IsSavedMenuText
        {
            get
            {
                return IsSaved ? resourceMap.GetValue("UnsavePost", resourceContext).ValueAsString : resourceMap.GetValue("SavePost", resourceContext).ValueAsString
;
            }
        }

        /// <summary>
        /// Sets text for a context menu item
        /// </summary>
        [JsonIgnore]
        public string IsHiddenMenuText
        {
            get
            {
                return IsHidden ? resourceMap.GetValue("UnhidePost", resourceContext).ValueAsString : resourceMap.GetValue("HidePost", resourceContext).ValueAsString;
            }
        }

        /// <summary>
        /// Used by subreddit view to mark the title read
        /// </summary>
        [JsonIgnore]
        public SolidColorBrush TitleTextBrush
        {
            get
            {
                return new SolidColorBrush(TitleTextColor);
            }
        }

        /// <summary>
        /// The color this post's upvote button should be in the UI.
        /// It is accented if and only if the user has upvoted this comment.
        /// </summary>
        [JsonIgnore]
        public SolidColorBrush UpVoteColor
        {
            get
            {
                if(Likes.HasValue && Likes.Value)
                {
                    return GetAccentBrush();
                }
                else
                {
                    return new SolidColorBrush(s_colorGray);
                }
            }
        }

        /// <summary>
        /// The color this post's downvote button should be in the UI.
        /// It is accented if and only if the user has upvoted this comment.
        /// </summary>
        [JsonIgnore]
        public SolidColorBrush DownVoteColor
        {
            get
            {
                if (Likes.HasValue && !Likes.Value)
                {
                    return GetAccentBrush();
                }
                else
                {
                    return new SolidColorBrush(s_colorGray);
                }
            }
        }

        /// <summary>
        /// The color the UI should use to indicate there are unread comments on this post.
        /// </summary>
        [JsonIgnore]
        public SolidColorBrush NewCommentColor
        {
            get
            {
                if (String.IsNullOrWhiteSpace(NewCommentText))
                {
                    return new SolidColorBrush(s_colorGray);
                }
                else
                {
                    return GetLightenedAccentBrush();
                }
            }
        }

        /// <summary>
        /// A darker accented color.
        /// </summary>
        [JsonIgnore]
        public SolidColorBrush DarkenedAccentColorBrush
        {
            get
            {
                return GetDarkenedAccentBrush();
            }
        }

        /// <summary>
        /// The spacing to leave before the new-comments indicator.
        /// This is empty if there are no new comments.
        /// </summary>
        [JsonIgnore]
        public Thickness NewCommentMargin
        {
            get
            {
                if (String.IsNullOrWhiteSpace(NewCommentText))
                {
                    return new Thickness(0);
                }
                else
                {
                    return new Thickness(0, 0, 3, 0);
                }
            }
        }

        /// <summary>
        /// Used by the subreddit list to show or hide sticky
        /// </summary>
        [JsonIgnore]
        public Visibility StickyVisibility
        {
            get
            {
                return IsStickied ? Visibility.Visible: Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Used by the subreddit list to show or hide link flair
        /// </summary>
        [JsonIgnore]
        public Visibility FlairVisibility
        {
            get
            {
                return String.IsNullOrWhiteSpace(LinkFlairText) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        /// <summary>
        /// Used by the subreddit list to show or hide gilded tag
        /// </summary>
        [JsonIgnore]
        public Visibility GildedVisibility
        {
            get
            {
                return Gilded ? Visibility.Visible : Visibility.Collapsed;
            }
        }



        #region FlipView Vars


        /// <summary>
        /// The visibility of "Loading Comments", depending on if the comments have loaded yet.
        /// </summary>
        [JsonIgnore]
        public Visibility ShowCommentLoadingMessage
        {
            get
            {
                return m_showCommentLoadingMessage;
            }
            set
            {
                SetProperty(ref m_showCommentLoadingMessage, value);
            }
        }
        [JsonIgnore]
        Visibility m_showCommentLoadingMessage = Visibility.Collapsed;

        /// <summary>
        /// A string that is used to show errors in the footer of the comment list. If "" it is hidden.
        /// </summary>
        [JsonIgnore]
        public string ShowCommentsErrorMessage
        {
            get
            {
                return m_showCommentsErrorMessage;
            }
            set
            {
                SetProperty(ref m_showCommentsErrorMessage, value);
            }
        }
        [JsonIgnore]
        string m_showCommentsErrorMessage = "";

        /// <summary>
        /// The visibility of the menu button when the post is displayed in flip view.
        /// </summary>
        [JsonIgnore]
        public Visibility FlipViewMenuButton
        {
            get
            {
                return m_flipViewMenuButton;
            }
            set
            {
                SetProperty(ref m_flipViewMenuButton, value);
            }
        }
        [JsonIgnore]
        Visibility m_flipViewMenuButton = Visibility.Collapsed;

        /// <summary>
        /// The visibility of the button to show all comments on a post.
        /// This should be visible when only some comments are visible.
        /// </summary>
        [JsonIgnore]
        public Visibility FlipViewShowEntireThreadMessage
        {
            get
            {
                return m_flipViewShowEntireThreadMessage;
            }
            set
            {
                SetProperty(ref m_flipViewShowEntireThreadMessage, value);
            }
        }
        [JsonIgnore]
        Visibility m_flipViewShowEntireThreadMessage = Visibility.Collapsed;

        /// <summary>
        ///  Used to indicate if the save image option should be visible
        /// </summary>
        [JsonIgnore]
        public Visibility ShowSaveImageMenu { get; set; }

        /// <summary>
        /// Used by flip view to show the comment or post reply box
        /// </summary>
        [JsonIgnore]
        public string CommentingOnId
        {
            get
            {
                return m_commentingOnId;
            }
            set
            {
                SetProperty(ref m_commentingOnId, value);
            }
        }
        [JsonIgnore]
        string m_commentingOnId = "";


        /// <summary>
        /// Indicates how many comments we are showing
        /// </summary>
        [JsonIgnore]
        public int CurrentCommentShowingCount
        {
            get
            {
                return m_currentCommentCount;
            }
            set
            {
                SetProperty(ref m_currentCommentCount, value);
            }
        }
        [JsonIgnore]
        int m_currentCommentCount = 150;

        /// <summary>
        /// Shows or hides the loading more progress bar for comments.
        /// </summary>
        [JsonIgnore]
        public bool FlipViewShowLoadingMoreComments
        {
            get
            {
                return m_flipViewShowLoadingMoreComments;
            }
            set
            {
                if (SetProperty(ref m_flipViewShowLoadingMoreComments, value))
                {
                    OnPropertyChanged(nameof(FlipViewShowLoadingMoreCommentsVis));
                }
            }
        }
        [JsonIgnore]
        bool m_flipViewShowLoadingMoreComments = false;

        /// <summary>
        /// Indicates if the post is owned by the current user.
        /// </summary>
        [JsonIgnore]
        public bool IsPostOwnedByUser
        {
            get
            {
                return m_isPostOwnedByUser;
            }
            set
            {
                if (SetProperty(ref m_isPostOwnedByUser, value))
                {
                    OnPropertyChanged(nameof(DeletePostVisibility));
                    OnPropertyChanged(nameof(EditPostVisibility));
                }
            }
        }
        [JsonIgnore]
        bool m_isPostOwnedByUser = false;

        /// <summary>
        /// Used by the flip view to indicate if this post can be deleted by this user.
        /// </summary>
        [JsonIgnore]
        public Visibility DeletePostVisibility
        {
            get
            {
                return IsPostOwnedByUser ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Used by the flip view to indicate if this post can be edited by this user.
        /// </summary>
        [JsonIgnore]
        public Visibility EditPostVisibility
        {
            get
            {
                return IsPostOwnedByUser && IsSelf ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Shows or hides the loading more progress bar for comments.
        /// </summary>
        [JsonIgnore]
        public Visibility FlipViewShowLoadingMoreCommentsVis
        {
            get
            {
                return m_flipViewShowLoadingMoreComments ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion
    }
}
