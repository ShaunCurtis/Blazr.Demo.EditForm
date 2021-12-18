/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.UI;

public interface IModalDialog
{
    public ModalOptions Options { get; }

    public Task<ModalResult> ShowAsync<TModal>(ModalOptions options) where TModal : IComponent;

    public void Dismiss();

    public void Close(ModalResult result);

    public void Update(ModalOptions? options = null);

    public bool Display { get; }
}

