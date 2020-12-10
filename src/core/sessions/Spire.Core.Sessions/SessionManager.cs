#region

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using Spire.Core.Sessions.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#endregion

namespace Spire.Core.Sessions
{
    /// <summary>
    /// Base implementation of <see cref="ISessionManager"/>.
    /// </summary>
    public class SessionManager : ISessionManager
    {
        private readonly ConcurrentDictionary<int, IList<ISession>> _sessions;

        /// <summary>
        /// Creates new session manager.
        /// </summary>
        public SessionManager()
        {
            _sessions = new ConcurrentDictionary<int, IList<ISession>>();
        }

        /// <summary>
        /// Enumerates all the sessions, related to the current session manager.
        /// </summary>
        /// <returns>Sessions collection.</returns>
        public IEnumerable<ISession> GetSessions()
        {
            return _sessions.Values.SelectMany(x => x);
        }

        /// <summary>
        /// Adds a new session.
        /// </summary>
        /// <param name="session">Session to add.</param>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        public void AddSession<TEntity>(ISession<TEntity> session)
        {
            if (_sessions.TryGetValue(session.Owner.Id, out IList<ISession> existingSessions))
            {
                foreach (ISession existingSession in existingSessions)
                {
                    if (existingSession is ISession<TEntity> typedSession &&
                        typedSession.Owner.Id == session.Owner.Id && typedSession.EntityType == session.EntityType)
                    {
                        existingSessions.RemoveAt(existingSessions.IndexOf(existingSession));
                    }

                    existingSessions.Add(session);
                }
            }
            else
            {
                _sessions[session.Owner.Id] = new List<ISession>() {session};
            }
        }

        /// <summary>
        /// Creates a new session without adding it to the session manager.
        /// </summary>
        /// <param name="owner">Session owner.</param>
        /// <param name="entityType">Session entity type.</param>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        /// <returns>Created session</returns>
        public ISession<TEntity> CreateSession<TEntity>(User owner, UpdateType entityType)
        {
            return new Session<TEntity>(owner, entityType, Channel.CreateUnbounded<TEntity>());
        }

        /// <summary>
        /// Checks a session for existence.
        /// </summary>
        /// <param name="owner">Session owner.</param>
        /// <param name="entityType">Session entity type.</param>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        /// <returns>Session existence.</returns>
        public bool IsSessionExists<TEntity>(User owner, UpdateType entityType)
        {
            if (_sessions.TryGetValue(owner.Id, out IList<ISession> existingSessions))
            {
                foreach (ISession existingSession in existingSessions)
                {
                    if (existingSession is ISession<TEntity> typedSession &&
                        typedSession.Owner.Id == owner.Id && typedSession.EntityType == entityType)
                    {
                        return true;
                    }
                }

                return false;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a session. Returns <see langword="null"/>, if session doesn't exists.
        /// </summary>
        /// <param name="owner">Session owner.</param>
        /// <param name="entityType">Session entity type.</param>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        /// <returns>A session.</returns>
        public ISession<TEntity> GetSession<TEntity>(User owner, UpdateType entityType)
        {
            if (_sessions.TryGetValue(owner.Id, out IList<ISession> existingSessions))
            {
                foreach (ISession existingSession in existingSessions)
                {
                    if (existingSession is ISession<TEntity> typedSession &&
                        typedSession.Owner.Id == owner.Id && typedSession.EntityType == entityType)
                    {
                        return typedSession;
                    }
                }

                return null;
            }
            else
            {
                return null;
            }
        }
    }
}