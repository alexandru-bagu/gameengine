using OpenTK.Audio.OpenAL;
using System;

namespace GameEngine.Audio
{
    public class ALException : Exception
    {
        public ALException() { }
        public ALException(string message) : base(message) { }
    }

    public class ALErrorStack
    {
        public static void Check()
        {
            ALError error = AL.GetError();
            if (error != ALError.NoError)
                throw new ALException(AL.GetErrorString(error));
        }
    }
}
