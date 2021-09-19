using System;
using System.Collections.Generic;
using Docker.DotNet;
using Docker.DotNet.Models;
using static Habitat.Cli.Utils.Strings;

namespace Habitat.Cli.Docker
{
    public static class Query
    {
        public static Func<ContainerListResponse, bool> ContainerNamed(string name) {
            return l => l.Names.Contains($"/{name}");
        }

        public static Func<NetworkResponse, bool> NetworkNamed(string name) {
            return n => n.Name.Equals(name);
        }

        public static Func<VolumeResponse, bool> VolumeNamed(string name) {
            return n => n.Name.Equals(name);
        }

        public static Func<ImagesListResponse, bool> ImageNamed(string name) {
            return i => i.RepoTags.Contains($"{name}");
        }

        public static void AddBasicFilter(IDictionary<string, IDictionary<string, bool>> filters,
                                          string key,
                                          string value) {
            if (IsBlank(value)) return;
            var nameFilter = new Dictionary<string, bool>
            {
                { value, true }
            };
            filters.Add(key, nameFilter);
        }
    }
}
