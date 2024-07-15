
using System;
using System.IO;
using System.Media;


namespace RCabinet.Helpers
{
  public sealed class SoundHelper
  {
    private static readonly string SOUND_PATH = Environment.CurrentDirectory;

    private static void Play(string fileName)
    {
      if (!(fileName.Trim() != string.Empty))
        return;
      SoundPlayer soundPlayer = new SoundPlayer();
      soundPlayer.SoundLocation = fileName;
      soundPlayer.Load();
      soundPlayer.Play();
    }

    public static void PlaySoundError()
    {
      string str = SoundHelper.SOUND_PATH + "\\Sounds\\fail.wav";
      if (!File.Exists(str))
        return;
      SoundHelper.Play(str);
    }

    public static void PlaySoundUnmatch()
    {
      string str = SoundHelper.SOUND_PATH + "\\Sounds\\unmatch.wav";
      if (!File.Exists(str))
        return;
      SoundHelper.Play(str);
    }

    public static void PlaySoundOK()
    {
      string str = SoundHelper.SOUND_PATH + "\\Sounds\\done.wav";
      if (!File.Exists(str))
        return;
      SoundHelper.Play(str);
    }
  }
}
