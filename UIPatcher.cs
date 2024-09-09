/*
    AutoHackGame
    Copyright (C) 2024  Alexandre 'kidev' Poumaroux

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using DiffMatchPatch;

namespace AutoHackGame;

public class UIPatcher
{
    private readonly string _baseDirectory;
    private readonly string _modsDirectory;
    private readonly diff_match_patch _dmp;
    private readonly List<string> _patchedFiles;

    public UIPatcher(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
        _modsDirectory = Path.Combine(_baseDirectory, "mods");
        _dmp = new diff_match_patch();
        _patchedFiles = new List<string>();
    }

    public async Task DownloadAndApplyPatchesAsync(string patchZipUrl)
    {
        string patchZipPath = await DownloadFileAsync(patchZipUrl);
        ExtractZip(patchZipPath, _modsDirectory);

        ApplyPatches(_modsDirectory);
        ApplySpecialPatchToGateway();
    }

    private async Task<string> DownloadFileAsync(string url)
    {
        using var client = new HttpClient();
        byte[] fileBytes = await client.GetByteArrayAsync(url);
        string filePath = Path.Combine(_baseDirectory, "patch.zip");
        await File.WriteAllBytesAsync(filePath, fileBytes);
        return filePath;
    }

    private void ExtractZip(string zipPath, string extractPath)
    {
        ZipFile.ExtractToDirectory(zipPath, extractPath, true);
        File.Delete(zipPath);
    }

    private void ApplyPatches(string directory)
    {
        foreach (string filePath in Directory.GetFiles(directory, "*.patch", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(_modsDirectory, filePath);
            string originalFilePath = Path.Combine(_baseDirectory, relativePath.Replace(".patch", ""));
            string patchedFilePath = Path.Combine(Path.GetDirectoryName(originalFilePath) ?? string.Empty, $"__{Path.GetFileName(originalFilePath)}");

            string patchContent = File.ReadAllText(filePath);
            string originalContent = File.Exists(originalFilePath) ? File.ReadAllText(originalFilePath) : "";

            List<Patch> patches = _dmp.patch_fromText(patchContent);
            Object[] patchResult = _dmp.patch_apply(patches, originalContent);
            string patchedContent = (string)patchResult[0];

            File.WriteAllText(patchedFilePath, patchedContent);

            // Add the patched file to the list (excluding gateway.html)
            if (Path.GetFileName(originalFilePath) != "gateway.html")
            {
                _patchedFiles.Add(relativePath.Replace(".patch", ""));
            }
        }
    }

    private void ApplySpecialPatchToGateway()
    {
        string gatewayPath = Path.Combine(_baseDirectory, "gateway.html");
        string patchedGatewayPath = Path.Combine(_baseDirectory, "__gateway.html");

        if (File.Exists(patchedGatewayPath))
        {
            string content = File.ReadAllText(patchedGatewayPath);

            foreach (string patchedFile in _patchedFiles)
            {
                string originalPath = patchedFile;
                string patchedPath = Path.Combine(Path.GetDirectoryName(patchedFile) ?? string.Empty, $"__{Path.GetFileName(patchedFile)}");

                content = Regex.Replace(content, Regex.Escape(originalPath), patchedPath);
            }

            File.WriteAllText(patchedGatewayPath, content);
        }
    }

    public void CleanupPatchedFiles()
    {
        foreach (string filePath in Directory.GetFiles(_baseDirectory, "__*", SearchOption.AllDirectories))
        {
            File.Delete(filePath);
        }
    }
}