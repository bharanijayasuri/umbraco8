using System;
using System.IO;
using System.Linq;
using System.Reflection;
//using Humanizer;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
//using Archetype.Models;
using Newtonsoft.Json;
//using Umbraco.Web.Extensions;
using Umbraco.Core.Logging;
using Umbraco.Web;
using System.Web;
using System.Web.Hosting;
using Umbraco.Web.Security;
using Umbraco.Core.Configuration;
using Umbraco.Web.Routing;
using Umbraco.Core.Services.Implement;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace UWS.Core.ApplicationEvents
{
    public class RelationsEventComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            //composition.SetContentLastChanceFinder();
            composition.Components().Append<RelationsEventHandlerComponent>();
        }
    }

    public class RelationsEventHandlerComponent : IComponent
    {
        
        private const string documentToMediaRelationTypeAlias = "DocumentToMedia";
        private const string documentToDocumentRelationTypeAlias = "DocumentToDocument";

        private static readonly Regex LocalLinkPattern = new Regex(@"href=""[/]?(?:\{|\%7B)localLink:([a-zA-Z0-9-://]+)(?:\}|\%7D)",
            RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        public List<int> contentIds = new List<int>();
        public List<int> mediaIds = new List<int>();

        public List<string> getMediaContentPropertyAliases()
        {
            return new List<string>() { "previewImage", "headerImage", "headerVideoLoop", "socialShareImage", "relatedPages", "headerVideoLoopSearch", "campaignHeaderImage", "newsPicker", "headerArticle", "customProfileImage", "accreditations", "promotedEvent","column2QuickLinks" };
        }

        IContentService contentService;
        IMediaService mediaService;
        IRelationService relationService;

        public List<KeyValuePair<string,string>> getMediaContentPropertyAliasesInArchetypes()
        {
            var keyvaluepairs = new List<KeyValuePair<string, string>>();
            keyvaluepairs.Add(new KeyValuePair<string, string>("pagePicker","multinodetreepicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("backgroundImage", "mediapicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("optionalBackgroundVideo", "mediapicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("text", "rte"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("image", "mediapicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("videoStartCard", "mediapicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("videoRolloverPreview", "mediapicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("video", "mediapicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("sharedContentBlockPicker", "contentpicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("imagePicker", "mediapicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("document", "mediapicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("icon", "mediapicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("newsButtonLink", "contentpicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("eventsButtonLink", "contentpicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("newsRoot", "contentpicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("eventsroot", "contentpicker"));
            //keyvaluepairs.Add(new KeyValuePair<string, string>("viewAllLink", ""));
            keyvaluepairs.Add(new KeyValuePair<string, string>("pickStaffMembers", "multinodetreepicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("relatedCourses", "multinodetreepicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("customProfileImage", "mediapicker"));

            //sub blocks
            keyvaluepairs.Add(new KeyValuePair<string, string>("itemText", "rte"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("content", "rte"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("smallPrint", "rte"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("smallImage", "mediapicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("sharedItemPicker", "contentpicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("description", "rte"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("sectionDescription", "rte"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("sharedContentItem", "multinodetreepicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("tabMainBodyText", "rte"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("tabbedItems", "rte"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("pageLinks", "urlpicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("pagePicker", "urlpicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("optionalLinks", "urlpicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("optionalCTAButton", "urlpicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("button1Link", "urlpicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("button2Link", "urlpicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("link", "urlpicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("viewAllLink", "urlpicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("buttonCTALink", "urlpicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("leftCTA", "urlpicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("rightCTA", "urlpicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("buttonCTA", "urlpicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("ctaLink", "urlpicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("optionalLinksColumn1", "urlpicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("allCoursesLink", "urlpicker"));
            keyvaluepairs.Add(new KeyValuePair<string, string>("column2QuickLinks", "urlpicker"));



            return keyvaluepairs;
            
        }

        

        public void Initialize()//This replaces the old OnApplicationStarted
        {
            //throw new NotImplementedException();

            ContentService.Saved += ContentService_Saved;
            MediaService.Trashing += MediaService_Trashing;
            ContentService.Trashing += ContentService_Trashing;

            contentService = Current.Services.ContentService;
            relationService = Current.Services.RelationService;
            mediaService = Current.Services.MediaService;

            createRelations();
        }

        void createRelations()
        {
            try
            {

                var relationService = Current.Services.RelationService;

                if (relationService.GetRelationTypeByAlias("DocumentToDocument") == null && relationService.GetRelationTypeByAlias("DocumentToMedia") == null)
                { 
                    var relationTypeDocumentToMedia = new RelationType(UmbracoObjectTypes.Media.GetGuid(),UmbracoObjectTypes.Document.GetGuid(), "DocumentToMedia", "Document To Media") { IsBidirectional = true };
                    var relationTypeDocumentToDocument = new RelationType(UmbracoObjectTypes.Document.GetGuid(), UmbracoObjectTypes.Document.GetGuid(), "DocumentToDocument", "Document To Document") { IsBidirectional = true };
                    relationService.Save(relationTypeDocumentToMedia);
                    relationService.Save(relationTypeDocumentToDocument);
                    Current.Logger.Info(this.GetType(),"Created relations DocumentToMedia and DocumentToDocument");
                }
            }
            catch (Exception msg)
            {
                Current.Logger.Debug(this.GetType(), "An  error occurred while creating the new relations for Related content app");
                Current.Logger.Error(this.GetType(), msg);
            }
        }

        public void Terminate()
        {
            throw new NotImplementedException();
        }


        private void ContentService_Trashing(IContentService sender, MoveEventArgs<IContent> e)
        {
            //relationService = ApplicationContext.Current.Services.RelationService;
            foreach (var entity in e.MoveInfoCollection)
            {
                if (relationService.GetByParentOrChildId(entity.Entity.Id).Any())
                {
                    //e.Cancel = true;
                    e.CancelOperation(new Umbraco.Core.Events.EventMessage("UWS", "Cannot be deleted because the content is used elsewhere in the site. Please check the related content section for more information.", Umbraco.Core.Events.EventMessageType.Error));
                    //e.Messages.Add(new Umbraco.Core.Events.EventMessage("UWS", "Cannot be deleted because the content is used elsewhere in the site. Please check the related content section for more information.", Umbraco.Core.Events.EventMessageType.Success));
                    break;
                }
            }
        }

        private void MediaService_Trashing(IMediaService sender, MoveEventArgs<IMedia> e)
        {
            //relationService = ApplicationContext.Current.Services.RelationService;
            foreach (var entity in e.MoveInfoCollection)
            {
                if (relationService.GetByParentOrChildId(entity.Entity.Id).Any())
                {
                    //e.Cancel = true;
                    e.CancelOperation(new Umbraco.Core.Events.EventMessage("UWS", "Cannot be deleted because the content is used elsewhere in the site. Please check the related content section for more information.", Umbraco.Core.Events.EventMessageType.Error));
                    //e.Messages.Add(new Umbraco.Core.Events.EventMessage("UWS", "Cannot be deleted because the content is used elsewhere in the site. Please check the related content section for more information.", Umbraco.Core.Events.EventMessageType.Success));
                    break;
                }
            }
        }

        private bool checkRelationship(int parentNode, int childNode)
        {
            /*
             *Checking whether parent and child items are proper content/media items doesn't work for media. 
             *MediaServie hits the Examine Indexes first and if the indexes does not exist, hits the database next.
             *Any new media item that gets created takes a while to appear in the indexes which makes MediaService not return that 
             item in the Descendants list. This makes the "check if the parentNode and childNode are actual content/media items in the website"
             below return the wrong value as if the media is not a proper media item.
             *The logic needs to be removed and it is okay to just check the existence of the relationship directly.
             */

        

            Current.Logger.Info(this.GetType(), string.Format("Checking if parent: {0} and child: {1} are related", parentNode, childNode));

            var result =  relationService.AreRelated(parentNode, childNode);

            if(result)
                Current.Logger.Info(this.GetType(), string.Format("Parent: {0} and child: {1} are related", parentNode, childNode));
            else
                Current.Logger.Info(this.GetType(), string.Format("Parent: {0} and child: {1} are not related", parentNode, childNode));


            return result;
        }

        private string getNodeIdFromUdi(string media)
        {
            Current.Logger.Info(this.GetType(),"getNodeIdFromUdi(string media): value in media: "+media);
            var id = string.Empty;
            try
            {
                var udi = GuidUdi.Parse(media);
                //var typedContent = udi.ToPublishedContent();
                var umbracoContext = UmbracoContext.Current == null ? EnsureUmbracoContext() : UmbracoContext.Current;
                var umbracoHelper = new UmbracoHelper(umbracoContext, Current.Services);

                if (udi.ToString().ToLower().Contains("document"))
                {
                    var typedContent = umbracoHelper.Content(udi);
                    id = typedContent?.Id.ToString();
                }
                else if (udi.ToString().ToLower().Contains("media"))
                {
                    var typedContent = umbracoHelper.Media(udi);
                    id = typedContent?.Id.ToString();
                }
                

                
            }
            catch (Exception msg)
            {
                Current.Logger.Error(this.GetType(), "Error while processing getNodeIdFromUdi(string media): value in media: " + media, msg);
            }
            return id;

        }

        private IRelationType getRelationType(int relationContentId)
        {

            
            var documentToMediaRelationType = relationService.GetRelationTypeByAlias(documentToMediaRelationTypeAlias);
            var documentToDocumentRelationType = relationService.GetRelationTypeByAlias(documentToDocumentRelationTypeAlias);

            var contentType = contentService.GetById(relationContentId);

            IRelationType relationType = null;

            //If contentType is not null, it's a page
            if (contentType != null)
            {
                relationType = documentToDocumentRelationType;
            }
            else//else it's a media item - most likely
            {
                relationType = documentToMediaRelationType;
            }

            return relationType;
        }

        private void ContentService_Saved(IContentService sender, SaveEventArgs<IContent> e)
        {
            
            var documentToMediaRelationType = relationService.GetRelationTypeByAlias(documentToMediaRelationTypeAlias);
            var documentToDocumentRelationType = relationService.GetRelationTypeByAlias(documentToDocumentRelationTypeAlias);


            var entitiesSaved = e.SavedEntities;

            foreach (var entity in entitiesSaved)
            {
                //Deletes all relation for parentID(entityID) before going on to recreate them later in the function
                foreach (var relation in relationService.GetByParentId(entity.Id))
                {
                    relationService.Delete(relation);
                }
                foreach (var prop in entity.Properties)
                {
                    try
                    {
                        if (prop.PropertyType.Name.Equals("contentBlocks") || prop.PropertyType.Name.Equals("FeatureBlocks"))
                        {
                            var propValue = prop?.GetValue();
                            if (propValue != null)
                            {
                                //processArchetypeBlocks(prop, entity);
                            }
                            else
                            {
                                continue;
                            }
                        }

                        if (/*getMediaContentPropertyAliases().Contains(prop.PropertyType.Name)*/true)
                        {
                            var propValue = prop.GetValue();

                            if (propValue == null) continue;

                            processRelations(prop, entity);
                        }
                    }
                    catch (Exception msg)
                    {
                        Current.Logger.Error(GetType(), "\n\nException while creating relation. Content id: " + entity.Id+". Entity  type: "+ entity.ContentType.Alias+". Property: "+prop?.Alias+". Property type: "+prop?.PropertyType.Alias,msg);
                        continue;
                    }
                }
            }
        }

        public void processRelations(Property prop, IContent entity)
        {

           

            var documentToMediaRelationType = relationService.GetRelationTypeByAlias(documentToMediaRelationTypeAlias);
            var documentToDocumentRelationType = relationService.GetRelationTypeByAlias(documentToDocumentRelationTypeAlias);

            var entityRelationsList = relationService.GetByParentId(entity.Id);

            var propValue = prop.GetValue();

            switch (prop.PropertyType.PropertyEditorAlias)
            {
               
                case Constants.PropertyEditors.Aliases.MediaPicker://obsolete, but still used for some properties in the site
                case Constants.PropertyEditors.Aliases.ContentPicker:
                //case Constants.PropertyEditors.ContentPickerAlias://obsolete, but still used for some properties in the site
                //case Constants.PropertyEditors.MediaPicker2Alias:
                    {
                        var mediaList = propValue.ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var media in mediaList)
                        {
                            var _media = media;
                            if (media != null && media.ToLower().Contains("umb"))
                            {
                                _media = getNodeIdFromUdi(media);

                            }

                            int relationContentId;
                            if (int.TryParse(_media.ToString(), out relationContentId))
                            {
                                if (!checkRelationship(entity.Id, relationContentId))
                                {

                                    var relationType = getRelationType(relationContentId);
                                    Current.Logger.Info(this.GetType(), string.Format("Creating relationship between parent: {0} and child: {1}",entity.Id, relationContentId));
                                    var relation = new Relation(entity.Id, relationContentId, relationType);
                                    relation.Comment = prop.PropertyType.Name;
                                    saveRelations(relation);
                                }
                                else
                                {
                                    Current.Logger.Info(this.GetType(), string.Format("Modifying existing relationship between parent: {0} and child: {1} to add an extra property alias {2} in the comment", entity.Id, relationContentId, prop.PropertyType.Name));
                                    IRelation relation = null;
                                    foreach (var _rel in entityRelationsList)
                                    {
                                        if (_rel.ChildId == relationContentId)
                                        {
                                            relation = _rel;
                                            break;
                                        }
                                    }

                                    if (relation != null)
                                    {
                                        relation.Comment += ", " + prop.PropertyType.Name;
                                        saveRelations(relation);
                                    }
                                }
                            }
                        }

                        break;
                    }
                case Constants.PropertyEditors.Aliases.MultiNodeTreePicker:
                    {
                        var mediaList = propValue.ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var media in mediaList)
                        {
                            var _media = media;
                            if (media != null && media.ToLower().Contains("umb"))
                            {
                                _media = getNodeIdFromUdi(media);

                            }

                            int relationContentId;
                            if (int.TryParse(_media, out relationContentId))
                            {
                                if (!checkRelationship(entity.Id, relationContentId))
                                {
                                    var relationType = getRelationType(relationContentId);
                                    Current.Logger.Info(this.GetType(), string.Format("Creating relationship between parent: {0} and child: {1}", entity.Id, relationContentId));
                                    var relation = new Relation(entity.Id, relationContentId, relationType);
                                    relation.Comment = prop.PropertyType.Name;
                                    saveRelations(relation);
                                }
                                else
                                {
                                    Current.Logger.Info(this.GetType(), string.Format("Modifying existing relationship between parent: {0} and child: {1} to add an extra property alias {2} in the comment", entity.Id, relationContentId, prop.PropertyType.Name));
                                    IRelation relation = null;
                                    foreach (var _rel in entityRelationsList)
                                    {
                                        if (_rel.ChildId == relationContentId)
                                        {
                                            relation = _rel;
                                            break;
                                        }
                                    }

                                    if (relation != null)
                                    {
                                        relation.Comment += ", " + prop.PropertyType.Name;
                                        saveRelations(relation);
                                    }
                                }
                            }
                        }
                        break;
                    }

                case Constants.PropertyEditors.Aliases.TinyMce:
                    {
                        var propertyValueAsString = propValue.ToString();

                        //This is one way to do the media:
                        var html = new HtmlDocument();
                        html.LoadHtml(propertyValueAsString);
                        //var imgTags = html.DocumentNode.FirstChild.SelectNodes(".//img");
                        var imgTags = html.DocumentNode.SelectNodes(".//img");
                        if (imgTags != null)
                        {
                            foreach (var img in imgTags)
                            {
                                //is element
                                if (img.NodeType != HtmlNodeType.Element) continue;
                                var mediaId = img.GetAttributeValue("rel", 0);
                                if (mediaId != 0)
                                {
                                    if (!checkRelationship(entity.Id, mediaId))
                                    {
                                        Current.Logger.Info(this.GetType(), string.Format("Creating relationship between parent: {0} and child: {1}", entity.Id, mediaId));
                                        var relation = new Relation(entity.Id, mediaId, documentToMediaRelationType);
                                        relation.Comment = prop.PropertyType.Name;
                                        saveRelations(relation);
                                    }
                                    else
                                    {
                                        Current.Logger.Info(this.GetType(), string.Format("Modifying existing relationship between parent: {0} and child: {1} to add an extra property alias {2} in the comment", entity.Id, mediaId, prop.PropertyType.Name));
                                        IRelation relation = null;
                                        foreach (var _rel in entityRelationsList)
                                        {
                                            if (_rel.ChildId == mediaId)
                                            {
                                                relation = _rel;
                                                break;
                                            }
                                        }

                                        if (relation != null)
                                        {
                                            relation.Comment += ", " + prop.PropertyType.Name;
                                            saveRelations(relation);
                                        }
                                    }
                                }
                                else if(!img.GetAttributeValue("data-udi", "def").Equals("def"))
                                {
                                    var umbUdi = img.GetAttributeValue("data-udi", "def");
                                    if (umbUdi != null && umbUdi.Contains("umb"))
                                    {
                                        var _mediaId = getNodeIdFromUdi(umbUdi);
                                        if (_mediaId != null && !checkRelationship(entity.Id, int.Parse(_mediaId)))
                                        {
                                            Current.Logger.Info(this.GetType(), string.Format("Creating relationship between parent: {0} and child: {1}", entity.Id, _mediaId));
                                            var relation = new Relation(entity.Id, int.Parse(_mediaId), documentToMediaRelationType);
                                            relation.Comment = prop.PropertyType.Name;
                                            saveRelations(relation);
                                        }
                                        else if (_mediaId != null)
                                        {
                                            Current.Logger.Info(this.GetType(), string.Format("Modifying existing relationship between parent: {0} and child: {1} to add an extra property alias {2} in the comment", entity.Id, _mediaId, prop.PropertyType.Name));
                                            IRelation relation = null;
                                            foreach (var _rel in entityRelationsList)
                                            {
                                                if (_rel.ChildId == int.Parse(_mediaId))
                                                {
                                                    relation = _rel;
                                                    break;
                                                }
                                            }

                                            if (relation != null)
                                            {
                                                relation.Comment += ", " + prop.PropertyType.Name;
                                                saveRelations(relation);
                                            }
                                        }

                                        else if (_mediaId == null)
                                        {
                                            Current.Logger.Info(this.GetType(), string.Format("Cannot create relationship between parent: {0} and child: {1} as media id for the umb UDI is null. Property in comment: {2}", entity.Id, umbUdi, prop.PropertyType.Name));
                                        }
                                    }
                                }

                                
                            }
                        }

                        //This is how you can parse locallinks
                        var tags = LocalLinkPattern.Matches(propertyValueAsString);
                        foreach (Match tag in tags)
                        {
                            if (tag.Groups.Count > 0)
                            {
                                var id = tag.Groups[1].Value;
                                int intId;
                                if (int.TryParse(id, out intId))
                                {
                                    if (!checkRelationship(entity.Id, intId))
                                    {
                                        Current.Logger.Info(this.GetType(), string.Format("Creating relationship between parent: {0} and child: {1}", entity.Id, intId));
                                        var relation = new Relation(entity.Id, intId, documentToDocumentRelationType);
                                        relation.Comment = prop.PropertyType.Name;
                                        saveRelations(relation);
                                    }
                                    else
                                    {
                                        Current.Logger.Info(this.GetType(), string.Format("Modifying existing relationship between parent: {0} and child: {1} to add an extra property alias {2} in the comment", entity.Id, intId, prop.PropertyType.Name));
                                        IRelation relation = null;
                                        foreach (var _rel in entityRelationsList)
                                        {
                                            if (_rel.ChildId == intId)
                                            {
                                                relation = _rel;
                                                break;
                                            }
                                        }

                                        if (relation != null)
                                        {
                                            relation.Comment += ", " + prop.PropertyType.Name;
                                            saveRelations(relation);
                                        }
                                    }
                                }
                            }
                        }

                        break;
                    }

            }
        }




       


        private void saveRelations(IRelation relation)
        {
            try
            {
                if (relation.ChildId != 0)
                {
                    relationService.Save(relation);
                }
                else
                {
                    Current.Logger.Info(this.GetType(), string.Format("Skipping creation of relation between parent: {0} and child: {1} because child ID is 0. Property in comment: {2} ", relation.ParentId, relation.ChildId, relation.Comment));
                }
            }
            catch (Exception msg)
            {
                Current.Logger.Error(this.GetType(), string.Format("Exception while creating relation between parent: {0} and child: {1}. Property in comment: {2} ", relation.ParentId, relation.ChildId, relation.Comment), msg);
            }
        }





        

        public static UmbracoContext EnsureUmbracoContext()
        {
            if (UmbracoContext.Current != null)
            {
                return UmbracoContext.Current;
            }
            var context = new HttpContextWrapper(new HttpContext(new SimpleWorkerRequest("/", string.Empty, new StringWriter())));

           

            return UmbracoContext.EnsureContext(
               Current.Factory.GetInstance<IUmbracoContextAccessor>(),
               context, /*find a way to include publishedSnapshotService*/ Umbraco.Web.Composing.Current.PublishedSnapshotService,
               new WebSecurity(context, Current.Services.UserService, Current.Configs.GetConfig<IGlobalSettings>()),
               Current.Configs.GetConfig<IUmbracoSettingsSection>(),
               /*Another way to construct the config manually() => { Configs config = null; Umbraco.Core.ConfigsExtensions.AddCoreConfigs(config); return config.GetConfig<IUmbracoSettingsSection>(); }*/
               /*UrlProviderResolver.Current.Providers,*/
               Enumerable.Empty<IUrlProvider>(),
               Current.Configs.GetConfig<IGlobalSettings>(), Current.VariationContextAccessor,
               false);

        }

    }
}
