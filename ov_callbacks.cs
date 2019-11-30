//
// GTA:SA ASI loader
// (c) 2007-2008 Stanislav "listener" Golovin
// 
// SharpASI © 2019, DartPower Team LLC

//=============================================================================
// vorbisfile connector
public class ov_callbacks
{
    public delegate uint read_funcDelegate(object ptr, uint size, uint nmemb, object datasource);
    public read_funcDelegate read_func;
    public delegate int seek_funcDelegate(object datasource, long offset, int whence);
    public seek_funcDelegate seek_func;
    public delegate int close_funcDelegate(object datasource);
    public close_funcDelegate close_func;
    public delegate int tell_funcDelegate(object datasource);
    public tell_funcDelegate tell_func;
}

public delegate int __ov_open_callbacksDelegate(object datasource, object vf, ref string initial, int ibytes, ov_callbacks callbacks);