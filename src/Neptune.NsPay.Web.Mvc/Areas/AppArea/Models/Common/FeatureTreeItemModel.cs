﻿namespace Neptune.NsPay.Web.Areas.AppArea.Models.Common
{
    public class FeatureTreeItemModel
    {
        public IFeatureEditViewModel EditModel { get; set; }

        public string ParentName { get; set; }

        public FeatureTreeItemModel()
        {
            
        }

        public FeatureTreeItemModel(IFeatureEditViewModel editModel, string parentName)
        {
            EditModel = editModel;
            ParentName = parentName;
        }
    }
}