﻿using CUE4Parse_Conversion.Formats.Materials;
using CUE4Parse_Conversion.Options;
using CUE4Parse.UE4.Assets.Exports.Material;
using CUE4Parse.UE4.Assets.Exports.Texture;

namespace CUE4Parse_Conversion.Exporters;

public sealed class MaterialExporter(UMaterialInterface material) : ExporterBase(material)
{
    protected override IReadOnlyList<ExportFile> BuildExportFiles(CancellationToken ct = default)
    {
        Log.Debug("Extracting material parameters (depth: {Depth})", Session.Options.MaterialDepth);

        var parameters = new CMaterialParams2();
        material.GetParams(parameters, Session.Options.MaterialDepth);

        var files = new List<ExportFile> { new JsonMaterialFormat().Build(ObjectName, parameters) };
        if (Session.Options.MeshFormat == EMeshFormat.USD)
        {
            files.Add(new UsdMaterialFormat().Build(ObjectName, parameters, SaveDirectory));
        }

        foreach (var ptr in parameters.Textures.Values)
        {
            ct.ThrowIfCancellationRequested();
            if (!ptr.TryLoad<UTexture>(out var texture)) continue;
            // render targets (e.g. a game's global RainShadowMap) are filled at runtime: the package has no pixels to export
            if (texture is UTextureRenderTarget) continue;

            Session.Add(texture);
        }

        return files;
    }
}
