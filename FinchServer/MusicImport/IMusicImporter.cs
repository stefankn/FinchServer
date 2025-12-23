using FinchServer.Beets;
using FinchServer.Components.Pages.Import;
using Microsoft.AspNetCore.Components.Forms;
using ImportJob = FinchServer.Database.ImportJob;

namespace FinchServer.MusicImport;

public interface IMusicImporter {
    
    // - Functions

    public Task<ImportJob> Upload(IBrowserFile[] files);
    public InteractiveCommand Import(ImportJob job, ImportConfiguration configuration);
}