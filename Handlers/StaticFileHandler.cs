﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomWebServer.Lib;
using System.IO;

namespace CustomWebServer.Handlers
{
    public class StaticFileHandler : IRequestHandler
    {
        private readonly DirectoryInfo _rootDirectory;
        private readonly String _defaultFileName;
        private readonly FileInfo _404;
        private readonly IDictionary<String, String> _contentTypes;

        public StaticFileHandler(String rootDirectory, String defaultFile = null)
        {
            _rootDirectory = new DirectoryInfo(rootDirectory);
            _defaultFileName = defaultFile ?? String.Empty;
            _404 = new FileInfo(Path.Combine(_rootDirectory.FullName, "404.html"));
            _contentTypes = SetupContentTypes();
        }

        public async Task<IResponse> HandleRequest(IRequest request)
        {
            var fullPath = CreateFilePath(request);
            var fileInfo = new FileInfo(fullPath);

            if (fileInfo.Exists)
            {
                return new Response(200, "OK", CreateResponseHeaders(fileInfo), fileInfo.OpenRead());
            }

            return new Response(404, "Not Found", CreateResponseHeaders(_404), _404.OpenRead());
        }

        private IDictionary<String, Object> CreateResponseHeaders(FileInfo fileInfo)
        {
            return new Dictionary<String, Object>
                              {
                                  {"content-type", GetContentType(fileInfo.Extension)},
                                  {"content-length", fileInfo.Length}
                              };
        }

        private string CreateFilePath(IRequest request)
        {
            var rootPath = _rootDirectory.FullName;
            var virtualPath = request.RequestUri.LocalPath;

            if (!rootPath.EndsWith(@"\"))
            {
                rootPath += Path.DirectorySeparatorChar;
            }

            if (virtualPath.StartsWith(@"/"))
            {
                virtualPath = virtualPath.TrimStart(Path.AltDirectorySeparatorChar);
            }

            if (virtualPath == String.Empty)
            {
                virtualPath = _defaultFileName;
            }

            return Path.Combine(rootPath, virtualPath);
        }

        private String GetContentType(string extension)
        {
            return _contentTypes.ContainsKey(extension)
                       ? _contentTypes[extension]
                       : _contentTypes[".bin"];
        }

        private static IDictionary<string, string> SetupContentTypes()
        {
            return new Dictionary<string, string>
                       {
                           {".txt", "text/plain"},
                           {".htm", "text/html"},
                           {".html", "text/html"},
                           {".css", "text/css"},
                           {".js", "application/javascript"},
                           {".jpg", "image/jpeg"},
                           {".jpeg", "image/jpeg"},
                           {".gif", "image/gif"},
                           {".png", "image/png"},
                           {".bin", "application/octet-stream"}
                       };
        }
    }
}