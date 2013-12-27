using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SharepointCommon.Common;
using Assert = NUnit.Framework.Assert;

namespace SharepointCommon.Test.CustomFields
{

    public class CustomFieldTests
    {
        [Test]
        public void Ensure_List_With_Custom_Lookup()
        {
            /*Mockable.AddFieldAsXml = (collection, s) => string.Empty;
            Mockable.GetFieldByInternalName = (collection, s) => null;
            Mockable.FieldMapper_SetFieldProperties = (f, fi) => { };*/

            using (var tls = new TestListScope<Contract>("Ensure_List_With_Custom_Lookup"))
            {
                var list = tls.List;
            }
        }

        [Test]
        public void Add_Item_With_Custom_Lookup()
        {
            /*Mockable.AddFieldAsXml = (collection, s) => string.Empty;
            Mockable.GetFieldByInternalName = (collection, s) => null;
            Mockable.FieldMapper_SetFieldProperties = (f, fi) => { };*/

            using (var tls = new TestListScope<Contract>("Add_Item_With_Custom_Lookup"))
            {
                var list = tls.List;
                var i = new Contract
                {
                    ProjectNew = new Item { Id = 1, },
                    Projects = new List<Item> { new Item { Id = 1, }, new Item { Id = 2, } },
                };
                list.Add(i);
            }
        }


        [Test]
        public void Get_Custom_Lookup_Value_Throws_NoOverride_Test()
        {
            using (var tls = new TestListScope<ContractBad>("Get_Custom_Lookup_Value_Throws_NoOverride_Test"))
            {
                var list = tls.List;
                var i = new ContractBad { };
                list.Add(i);
                var item = list.ById(i.Id);

                Assert.Throws<SharepointCommonException>(() =>
                {
                    var project = item.Project;
                });
            }
        }

        [Test]
        public void Get_Custom_Lookup_Value_Test()
        {
            using (var tls = new TestListScope<Contract>("Get_Custom_Lookup_Value_Test"))
            {
                var list = tls.List;
                var i = new Contract 
                {
                    Projects = new List<Item> { new Item { Id = 1, }, new Item { Id = 2, } },
                };

                list.Add(i);
                var item = list.ById(i.Id);
                var project = item.ProjectNew;
                var projects = item.Projects.ToList();
            }
        }
    }
}
