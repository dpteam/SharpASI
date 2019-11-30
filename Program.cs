using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

static class Program
{
    private const string nativeLibName = "vorbisHooked.dll";

    [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr malloc(IntPtr size);

    [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void free(IntPtr memblock);

    public enum SeekWhence : int
    {
        SEEK_SET = 0,
        SEEK_CUR = 1,
        SEEK_END = 2
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr read_func(
        IntPtr ptr,
        IntPtr size,        
        IntPtr nmemb,       
        IntPtr datasource   
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int seek_func(
        IntPtr datasource,  
        long offset,        
        SeekWhence whence
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int close_func(
        IntPtr datasource   
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr tell_func(
        IntPtr datasource   
    );

    [StructLayout(LayoutKind.Sequential)]
    public struct vorbis_info
    {
        public int version;
        public int channels;
        public IntPtr rate;     
        public IntPtr bitrate_upper;    
        public IntPtr bitrate_nominal;  
        public IntPtr bitrate_lower;    
        public IntPtr bitrate_window;   
        public IntPtr codec_setup;  
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct vorbis_comment
    {
        public IntPtr user_comments;    
        public IntPtr comment_lengths;  
        public int comments;
        public IntPtr vendor;       
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ov_callbacks
    {
        public read_func read_func;
        public seek_func seek_func;
        public close_func close_func;
        public tell_func tell_func;
    }

    [DllImport(nativeLibName, EntryPoint = "ov_fopen", CallingConvention = CallingConvention.Cdecl)]
    private static extern int INTERNAL_ov_fopen(
        [In()] [MarshalAs(UnmanagedType.LPStr)]
            string path,
        IntPtr vf
    );
    public static int ov_fopen(string path, out IntPtr vf)
    {
        vf = AllocVorbisFile();
        return INTERNAL_ov_fopen(path, vf);
    }

    [DllImport(nativeLibName, EntryPoint = "ov_open_callbacks", CallingConvention = CallingConvention.Cdecl)]
    private static extern int INTERNAL_ov_open_callbacks(
        IntPtr datasource,
        IntPtr vf,
        IntPtr initial,
        IntPtr ibytes,
        ov_callbacks callbacks
    );
    public static int ov_open_callbacks(
        IntPtr datasource,  
        out IntPtr vf,
        IntPtr initial,     
        IntPtr ibytes,      
        ov_callbacks callbacks
    )
    {
        vf = AllocVorbisFile();
        return INTERNAL_ov_open_callbacks(
            datasource,
            vf,
            initial,
            ibytes,
            callbacks
        );
    }

    [DllImport(nativeLibName, EntryPoint = "ov_info", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr INTERNAL_ov_info(
        IntPtr vf,
        int link
    );
    public static vorbis_info ov_info(
        IntPtr vf,
        int link
    )
    {
        IntPtr result = INTERNAL_ov_info(vf, link);
        if (result == IntPtr.Zero) throw new InvalidOperationException("Specified bitstream does not exist or the file has been initialized improperly.");
        vorbis_info info = (vorbis_info)Marshal.PtrToStructure(
            result,
            typeof(vorbis_info)
        );
        return info;
    }

    [DllImport(nativeLibName, EntryPoint = "ov_comment", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr INTERNAL_ov_comment(
        IntPtr vf,
        int link
    );
    public static vorbis_comment ov_comment(
        IntPtr vf,
        int link
    )
    {
        IntPtr result = INTERNAL_ov_comment(vf, link);
        if (result == IntPtr.Zero) throw new InvalidOperationException("Specified bitstream does not exist or the file has been initialized improperly.");
        vorbis_comment comment = (vorbis_comment)Marshal.PtrToStructure(
            result,
            typeof(vorbis_comment)
        );
        return comment;
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern double ov_time_total(IntPtr vf, int i);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern long ov_pcm_total(IntPtr vf, int i);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ov_read(
        IntPtr vf,
        byte[] buffer,
        int length,
        int bigendianp,
        int word,
        int sgned,
        out int current_section
    );

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ov_read(
        IntPtr vf,
        IntPtr buffer,
        int length,
        int bigendianp,
        int word,
        int sgned,
        out int current_section
    );

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int ov_time_seek(IntPtr vf, double s);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int ov_pcm_seek(IntPtr vf, long s);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern double ov_time_tell(IntPtr vf);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern long ov_pcm_tell(IntPtr vf);

    [DllImport(nativeLibName, EntryPoint = "ov_clear", CallingConvention = CallingConvention.Cdecl)]
    private static extern int INTERNAL_ov_clear(IntPtr vf);
    public static int ov_clear(ref IntPtr vf)
    {
        int result = INTERNAL_ov_clear(vf);
        free(vf);
        vf = IntPtr.Zero;
        return result;
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ov_streams(IntPtr vf);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ov_seekable(IntPtr vf);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int ov_raw_seek(IntPtr vf, IntPtr pos);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ov_raw_tell(IntPtr vf);

    private static IntPtr AllocVorbisFile()
    {
        const int size32 = 720;
        const int size64Unix = 944;
        const int size64Windows = 840;

        PlatformID platform = Environment.OSVersion.Platform;
        if (IntPtr.Size == 4)
        {
            return malloc((IntPtr)size32);
        }
        if (IntPtr.Size == 8)
        {
            if (platform == PlatformID.Unix)
            {
                return malloc((IntPtr)size64Unix);
            }
            else if (platform == PlatformID.Win32NT)
            {
                return malloc((IntPtr)size64Windows);
            }
            throw new NotSupportedException("Unhandled platform!");
        }
        throw new NotSupportedException("Unhandled architecture!");
    }

    public static void vorbisAttach()
    {
        AllocVorbisFile();
    }

    public static void loadASI()
    {
        string binPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "sharpScripts");

        foreach (string dll in Directory.GetFiles(binPath, "*.asi", SearchOption.AllDirectories))
        {
            try
            {
                Assembly loadedAssembly = Assembly.LoadFile(dll);
            }
            catch (FileLoadException loadEx)
            { }
            catch (BadImageFormatException imgEx)
            { }
        }
    }

    const int DLL_PROCESS_ATTACH = 0;
    const int DLL_THREAD_ATTACH = 1;
    const int DLL_THREAD_DETACH = 2;
    const int DLL_PROCESS_DETACH = 3;

    static Action process_attach = () => Console.WriteLine(@"process_attach");
    static Action thread_attach = () => Console.WriteLine(@"thread_attach");
    static Action thread_detach = () => Console.WriteLine(@"thread_detach");
    static Action process_detach = () => Console.WriteLine(@"process_detach");

    public static int _fltused = 0x9875;

    [STAThread]
    public static int DllMain(System.IntPtr hModule, uint ul_reason_for_call, object lpReserved)
    {

        switch (ul_reason_for_call)
        {
            case DLL_PROCESS_ATTACH:
                process_attach();
                vorbisAttach();
                loadASI();
                break;
            case DLL_THREAD_ATTACH:
                thread_attach();
                break; 
            case DLL_THREAD_DETACH:
                thread_detach();
                break;
            case DLL_PROCESS_DETACH:
                process_detach();
                break;
        }
        return 1;
    }
    
    [System.Runtime.InteropServices.DllImport("vorbisHooked.dll")]
    internal static extern int ov_open_callbacks(object datasource, object vf, ref string initial, int ibytes, ov_callbacks callbacks);
    [System.Runtime.InteropServices.DllImport("vorbisHooked.dll")]
    internal static extern int ov_clear(object vf);
    [System.Runtime.InteropServices.DllImport("vorbisHooked.dll")]
    internal static extern double ov_time_total(object vf, int i);
    [System.Runtime.InteropServices.DllImport("vorbisHooked.dll")]
    internal static extern double ov_time_tell(object vf);
    [System.Runtime.InteropServices.DllImport("vorbisHooked.dll")]
    internal static extern int ov_read(object vf, ref string buffer, int length, int bigendianp, int word, int sgned, ref int bitstream);
    [System.Runtime.InteropServices.DllImport("vorbisHooked.dll")]
    internal static extern object ov_info(object vf, int link);
    [System.Runtime.InteropServices.DllImport("vorbisHooked.dll")]
    internal static extern int ov_time_seek(object vf, double pos);
}