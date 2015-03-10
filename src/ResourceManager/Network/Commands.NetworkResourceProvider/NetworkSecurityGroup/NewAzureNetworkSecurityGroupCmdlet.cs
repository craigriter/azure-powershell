﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using AutoMapper;
using Microsoft.Azure.Management.Network;
using Microsoft.Azure.Commands.NetworkResourceProvider.Models;
using Microsoft.Azure.Commands.NetworkResourceProvider.Properties;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using MNM = Microsoft.Azure.Management.Network.Models;

namespace Microsoft.Azure.Commands.NetworkResourceProvider
{
    [Cmdlet(VerbsCommon.New, "AzureNetworkSecurityGroup"), OutputType(typeof(PSNetworkSecurityGroup))]
    public class NewAzureNetworkSecurityGroupCmdlet : NetworkSecurityGroupBaseClient
    {
        [Alias("ResourceName")]
        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The resource name.")]
        [ValidateNotNullOrEmpty]
        public virtual string Name { get; set; }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The resource group name.")]
        [ValidateNotNullOrEmpty]
        public virtual string ResourceGroupName { get; set; }

        [Parameter(
         Mandatory = true,
         ValueFromPipelineByPropertyName = true,
         HelpMessage = "location.")]
        [ValidateNotNullOrEmpty]
        public virtual string Location { get; set; }

        [Parameter(
             Mandatory = false,
             ValueFromPipelineByPropertyName = true,
             HelpMessage = "The list of NetworkSecurityRules")]
        public List<PSSecurityRule> SecurityRules { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "An array of hashtables which represents resource tags.")]
        public Hashtable[] Tag { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Do not ask for confirmation if you want to overrite a resource")]
        public SwitchParameter Force { get; set; }

        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();

            if (this.IsNetworkSecurityGroupPresent(this.ResourceGroupName, this.Name))
            {
                ConfirmAction(
                    Force.IsPresent,
                    string.Format(Resources.OverwritingResource, Name),
                    Resources.OverwritingResourceMessage,
                    Name,
                    () => this.CreateNetworkSecurityGroup());
            }

            var virtualNetwork = this.CreateNetworkSecurityGroup();

            WriteObject(virtualNetwork);
        }

        private PSNetworkSecurityGroup CreateNetworkSecurityGroup()
        {
            var nsg = new PSNetworkSecurityGroup();
            nsg.Name = this.Name;
            nsg.ResourceGroupName = this.ResourceGroupName;
            nsg.Location = this.Location;
            nsg.Properties = new PSNetworkSecurityGroupProperties { SecurityRules = this.SecurityRules };

            // Map to the sdk object
            var nsgModel = Mapper.Map<MNM.NetworkSecurityGroupCreateOrUpdateParameters>(nsg);
            nsgModel.Tags = TagsConversionHelper.CreateTagDictionary(this.Tag, validate: true);

            // Execute the Create VirtualNetwork call
            this.NetworkSecurityGroupClient.CreateOrUpdate(this.ResourceGroupName, this.Name, nsgModel);

            var getNetworkSecurityGroup = this.GetNetworkSecurityGroup(this.ResourceGroupName, this.Name);

            return getNetworkSecurityGroup;
        }
    }
}
