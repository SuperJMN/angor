using AngorApp.Model;

namespace Angor.Model.Implementation.Projects;

public static class ProjectMapper
{
    /// <summary>
    /// Extensión para mapear de ProjectData a IProject.
    /// </summary>
    public static IProject ToProject(this ProjectData data)
    {
        if (data is null) 
            throw new ArgumentNullException(nameof(data));

        if (data.ProjectInfo is null)
            throw new ArgumentException("ProjectData.ProjectInfo no puede ser nulo.", nameof(data));

        if (data.NostrMetadata is null)
            throw new ArgumentException("ProjectData.NostrMetadata no puede ser nulo.", nameof(data));

        // Creamos la instancia de Project
        var project = new Project
        {
            // Usa la propiedad de ProjectIndexerData para el Id
            Id = data.IndexerData?.ProjectIdentifier,

            // Usamos la metadata para el Name, About (ShortDescription), Picture, etc.
            Name = data.NostrMetadata.Name,
            ShortDescription = data.NostrMetadata.About,

            Picture = string.IsNullOrWhiteSpace(data.NostrMetadata.Picture)
                ? null 
                : new Uri(data.NostrMetadata.Picture),

            Icon = string.IsNullOrWhiteSpace(data.NostrMetadata.Banner)
                ? null
                : new Uri(data.NostrMetadata.Banner),

            // Ejemplo: BitcoinAddress tomado de FounderRecoveryKey (puedes usar el que corresponda)
            BitcoinAddress = data.ProjectInfo.FounderRecoveryKey,

            TargetAmount = data.ProjectInfo.TargetAmount,
            StartingDate = DateOnly.FromDateTime(data.ProjectInfo.StartDate),
            PenaltyDuration = TimeSpan.FromDays(data.ProjectInfo.PenaltyDays),

            NpubKey = data.ProjectInfo.NostrPubKey,
            NpubKeyHex = data.IndexerData?.NostrEventId,

            InformationUri = string.IsNullOrWhiteSpace(data.NostrMetadata.Website)
                ? null
                : new Uri(data.NostrMetadata.Website)
        };

        // Mapeo de stages
        project.Stages = data.ProjectInfo.Stages
            .Select((stage, index) => new Stage
            {
                ReleaseDate = DateOnly.FromDateTime(stage.ReleaseDate),
                Amount = (uint) stage.AmountToRelease,  // Maneja la conversión decimal -> uint
                Index = index,
                Weight = project.TargetAmount > 0
                    ? (double)(stage.AmountToRelease / project.TargetAmount)
                    : 0.0
            })
            .Cast<IStage>()
            .ToList();

        return project;
    }
}

// Implementación concreta de IStage