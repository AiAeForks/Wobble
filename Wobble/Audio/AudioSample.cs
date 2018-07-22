using System;
using System.IO;
using System.Threading.Tasks;
using ManagedBass;

namespace Wobble.Audio
{
    public class AudioSample : IDisposable
    {
        /// <summary>
        ///     The sample's id.
        /// </summary>
        public int Id { get; }

        /// <summary>
        ///     The default concurrent plays for this sample.
        /// </summary>
        public const int DEFAULT_CONCURRENCY = 10;

        /// <summary>
        ///     If the sample has already previously played.
        /// </summary>
        public bool HasPlayed { get; private set; }

        /// <summary>
        ///     Determines if the audio stream can only be played one time.
        /// </summary>
        public bool OnlyCanPlayOnce { get; private set; }

        /// <summary>
        ///     Creates an audio sample from a local file.
        /// </summary>
        /// <param name="path">The path of the audio file.</param>
        /// <param name="onlyCanPlayOnce">If this sample is set to only play one time. (Default is true)</param>
        /// <param name="concurrentPlaybacks">The amount of concurrent playbacks possible for this sample</param>
        /// <exception cref="FileNotFoundException"></exception>
        public AudioSample(string path, bool onlyCanPlayOnce = true, int concurrentPlaybacks = DEFAULT_CONCURRENCY)
        {
            OnlyCanPlayOnce = onlyCanPlayOnce;
            Id = Load(path, concurrentPlaybacks);
        }

        /// <summary>
        ///     Creates an audio sample from a byte array.
        /// </summary>
        /// <param name="data">The byte array data of the sample</param>
        /// <param name="onlycanPlayOnce">If this sample is set to play only one time. (Default is true)</param>
        /// <param name="concurrentPlaybacks">The amount of concurrent playbacks possible for this sample</param>
        public AudioSample(byte[] data, bool onlycanPlayOnce = true, int concurrentPlaybacks = DEFAULT_CONCURRENCY)
        {
            OnlyCanPlayOnce = onlycanPlayOnce;
            Id = Load(data, concurrentPlaybacks);
        }

        /// <summary>
        ///     Plays the audio sample.
        /// </summary>
        public void Play()
        {
            // Only allow the sample to play once if specified.
            if (HasPlayed && OnlyCanPlayOnce)
                throw new AudioEngineException($"You cannot play a sample more than once");

            // Create a new channel for the sample to play on.
            var id = Bass.SampleGetChannel(Id);

            // Play the sample.
            Bass.ChannelPlay(id);

            HasPlayed = true;
        }

        /// <summary>
        ///     Loads an audio sample from a file path and returns an Id to it.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="concurrentPlaybacks"></param>
        /// <returns></returns>
        private static int Load(string path, int concurrentPlaybacks)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("AudioSample not found", path, new AudioEngineException());

            return Bass.SampleLoad(path, 0, 0, concurrentPlaybacks, BassFlags.Default | BassFlags.SampleOverrideLongestPlaying);
        }

        /// <summary>
        ///     Loads an audio sample from a byte array and returns an Id to the sample.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="concurrentPlaybacks"></param>
        /// <returns></returns>
        private static int Load(byte[] data, int concurrentPlaybacks) => Bass.SampleLoad(data, 0, data.Length, concurrentPlaybacks,
                                                                            BassFlags.Default | BassFlags.SampleOverrideLongestPlaying);
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Dispose() => Bass.SampleFree(Id);
    }
}