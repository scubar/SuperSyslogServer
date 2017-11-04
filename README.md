# SuperSyslogServer

Super Syslog Server is a high-performance Syslog server written in C#. Super Syslog Server uses an in-memory thread safe buffer to
store incoming messages and they are persisted to a database backend via a worker thread that periodically dequeues messages from
the buffer.

Super Syslog Server is not intended for production use at this time.
