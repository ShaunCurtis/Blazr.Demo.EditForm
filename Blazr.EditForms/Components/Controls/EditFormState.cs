/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================


namespace Blazr.EditForms;

/// <summary>
/// Component Class that adds Edit State to a Blazor EditForm Control
/// Should be placed within thr EditForm Control
/// Interacts with an EditStateContext
/// </summary>
public class EditFormState : ComponentBase, IDisposable
{
    private bool disposedValue;

    [CascadingParameter] public EditContext? EditContext { get; set; }

    [Parameter] public EditStateContext? EditStateContext { get; set; }

    protected override Task OnInitializedAsync()
    {
        // Ensure we have a EditContext and a EditStateContext
        Debug.Assert(this.EditContext != null);

        // check if we have been passed a EditStateContext as a Parameter
        // If not create one and load it with the EditContext
        if (EditStateContext is null)
            EditStateContext = new EditStateContext(this.EditContext);

        // Wire up FieldChanged to the EditContext OnFieldChanged event
        this.EditContext.OnFieldChanged += this.FieldChanged;

        return Task.CompletedTask;
    }

    // Pass the FieldChanged event on to the EditStateContext
    private void FieldChanged(object? sender, FieldChangedEventArgs e)
        => EditStateContext!.NotifyFieldChanged(e);

    // IDisposable Implementation
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (this.EditContext != null)
                    this.EditContext.OnFieldChanged -= this.FieldChanged;
            }
            //this.EditStateService.RecordSaved -= this.OnSave;
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

