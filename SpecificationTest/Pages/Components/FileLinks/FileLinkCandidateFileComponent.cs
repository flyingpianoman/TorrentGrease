using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using SpecificationTest.Crosscutting;

namespace SpecificationTest.Pages.Components.FileLinks
{
    internal class FileLinkCandidateFileComponent : IComponent<FileLinkCandidateFileComponent>
    {
        private readonly IWebElement _rootElement;
        /// <summary>
        /// Size + unit e.g. '25 kb'
        /// </summary>
        public string FilePath => _rootElement.FindElementByContentName("filepath").Text;
        public long DeviceID => long.Parse(_rootElement.FindElementByContentName("device-id").Text);
        public long InodeID => long.Parse(_rootElement.FindElementByContentName("inode-id").Text);

        public FileLinkCandidateFileComponent(IWebElement rootElement)
        {
            _rootElement  = rootElement;
        }

        public Task<FileLinkCandidateFileComponent> InitializeAsync()
        {
            return Task.FromResult(this);
        }
    }
}
