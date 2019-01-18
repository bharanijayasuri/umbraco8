using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
//using umbraco.BusinessLogic;
//using umbraco.interfaces;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
//using Umbraco.Web.PublishedContentModels;
using Current = Umbraco.Core.Composing.Current;


namespace UWS.Core.API
{
    [PluginController("UWSContentGovernanceRelatedContent")]
    public class UWSContentGovernanceRelatedContentController : UmbracoAuthorizedJsonController
    {

        public List<ReturnRelationModel> GetAllRelations(string id)
        {

            try
            {
                var relationService = Current.Services.RelationService;
                var contentService = Current.Services.ContentService;
                var mediaService = Current.Services.MediaService;

                var relations = relationService.GetByParentOrChildId(Int32.Parse(id));

                List<ReturnRelationModel> returnList = new List<ReturnRelationModel>();

                foreach (var relation in relations)
                {
                    var temp = new ReturnRelationModel();

                    var nodeName = getNodeName(relation.ParentId, contentService, mediaService);
                    temp.parentName = nodeName;
                    temp.parentID = relation.ParentId.ToString();
                    nodeName = getNodeName(relation.ChildId, contentService, mediaService);
                    temp.childName = nodeName;
                    temp.childID = relation.ChildId.ToString();
                    temp.relation = relation;
                    temp.comments = relation.Comment.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    temp.relation.RelationType = relationService.GetRelationTypeById(relation.RelationType.Id);

                    returnList.Add(temp);
                }

                return returnList;
            }
            catch (Exception msg)
            {
                return null;
            }

        }

        public string getNodeName(int id, IContentService contentService, IMediaService mediaService)
        {
            var nodeName = Umbraco.Content(id)?.Name;//get content from cache
            if (nodeName == null)//it is not in content cache
            {
                nodeName = Umbraco.Media(id)?.Name;//get media from cache

                if (nodeName == null)//it is not in media cache
                {
                    nodeName = contentService.GetById(id)?.Name+" (not published)";//get content from db using service
                    if (nodeName == null)//content is not in db 
                    {
                        nodeName = mediaService.GetById(id)?.Name;//get media from db
                    }
                }
            }
            
            //Return node name if name isn't null. Else return a message saying item can't be found
            return string.IsNullOrEmpty(nodeName)?"Item can't be found":nodeName;
        }

    }

    

    public class ReturnRelationModel
    {
        public string parentName { get; set; }
        public string parentID { get; set; }
        public string childName { get; set; }
        public string childID { get; set; }
        public string[] comments { get; set; }
        public IRelation relation { get; set; }

    }
}
