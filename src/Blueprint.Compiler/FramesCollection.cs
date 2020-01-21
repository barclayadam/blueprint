using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;

namespace Blueprint.Compiler
{
    public class FramesCollection : IReadOnlyList<Frame>
    {
        private readonly List<Frame> frames = new List<Frame>();

        /// <summary>
        /// Adds the specified <see cref="Frame" /> to this collection.
        /// </summary>
        /// <param name="frame">The non-null Frame.</param>
        /// <exception cref="ArgumentNullException">If the frame is <c>null</c>.</exception>
        public void Add(Frame frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            frames.Add(frame);
        }

        /// <summary>
        /// Inserts the specified <see cref="Frame" /> to this collection at the given index.
        /// </summary>
        /// <param name="index">The index at which to insert the frame.</param>
        /// <param name="frame">The non-null Frame.</param>
        /// <exception cref="ArgumentNullException">If the frame is <c>null</c>.</exception>
        public void Insert(int index, Frame frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            frames.Insert(index, frame);
        }

        /// <summary>
        /// Adds all of the given frames to this collection.
        /// </summary>
        /// <param name="framesToAdd">The frames to add.</param>
        /// <exception cref="ArgumentNullException">If any one of the frames to add is <c>null</c>.</exception>
        public void AddRange(IEnumerable<Frame> framesToAdd)
        {
            foreach (var frame in framesToAdd)
            {
                if (frame == null)
                {
                    throw new ArgumentNullException(nameof(frame));
                }

                frames.Add(frame);
            }
        }

        /// <summary>
        /// Clears all existing frames from this collection.
        /// </summary>
        public void Clear()
        {
            frames.Clear();
        }

        /// <summary>
        /// Adds a ReturnFrame to the method that will return a variable of the specified type.
        /// </summary>
        /// <param name="returnType">The type of variable to be returned.</param>
        /// <returns>This frame collection.</returns>
        public FramesCollection Return(Type returnType)
        {
            var frame = new ReturnFrame(returnType);

            Add(frame);
            return this;
        }

        /// <summary>
        /// Adds a ReturnFrame for the specified variable.
        /// </summary>
        /// <param name="returnVariable">The variable to be returned.</param>
        /// <returns>This frame collection.</returns>
        public FramesCollection Return(Variable returnVariable)
        {
            var frame = new ReturnFrame(returnVariable);

            Add(frame);
            return this;
        }

        /// <summary>
        /// Adds a ConstructorFrame{T} to the method frames.
        /// </summary>
        /// <param name="constructor"></param>
        /// <param name="configure">Optional, any additional configuration for the constructor frame.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>This frame collection.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public FramesCollection CallConstructor<T>(Expression<Func<T>> constructor, Action<ConstructorFrame<T>> configure = null)
        {
            var frame = new ConstructorFrame<T>(constructor);
            configure?.Invoke(frame);
            Add(frame);

            return this;
        }

        /// <summary>
        /// Add a frame to the end by its type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>This frame collection.</returns>
        public FramesCollection Append<T>() where T : Frame, new()
        {
            return Append(new T());
        }

        /// <summary>
        /// Append one or more frames to the end.
        /// </summary>
        /// <param name="frames"></param>
        /// <returns>This frame collection.</returns>
        public FramesCollection Append(params Frame[] frames)
        {
            AddRange(frames);
            return this;
        }

        /// <summary>
        /// Convenience method to add a method call to the GeneratedMethod Frames
        /// collection.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="configure">Optional configuration of the MethodCall.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>This frame collection.</returns>
        public FramesCollection Call<T>(Expression<Action<T>> expression, Action<MethodCall> configure = null)
        {
            var @call = MethodCall.For(expression);
            configure?.Invoke(@call);
            Add(@call);

            return this;
        }

        public void Write(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer)
        {
            foreach (var frame in this)
            {
                frame.GenerateCode(variables, method, writer);
            }
        }

        /// <inherit-doc />
        IEnumerator<Frame> IEnumerable<Frame>.GetEnumerator()
        {
            return frames.GetEnumerator();
        }

        /// <inherit-doc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)frames).GetEnumerator();
        }

        /// <inherit-doc />
        public int Count => frames.Count;

        /// <inherit-doc />
        public Frame this[int index] => frames[index];
    }
}
