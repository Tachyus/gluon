# Related

Google protocol-buffers and Apache Thrift are two industrial-strength
projects with fairly similar scope. The primary difference is that
these projects use a custom IDL language to generate both clients and
servers. They are also used for for serializing data to store, or else
in distributed systems that are not updated in lock-step, as we assume
in Gluon; such scenarios introduce the new requirement of being able
to extend service contracts without breaking existing code. Both
protocol-buffers and Thrift have a versioning facility to address
this.

Either project could be used instead of Gluon, however:

* protocol-buffers lacks TypeScript support and Thrift has it as
  third-party beta

* neither has F# support, though C# is probably working (so, no
  Unions, option, etc)

* the IDL approach is a bit more tailored for truly multiple-language
  projects; inferring schema from F# code is more convenient if that is
  the principal language

These projects should definitely be considered if requirements arise
of interacting with more languages such as Python or serializing
semi-structured data. It should be possible to extend future versions
of Gluon to, for example, generate Thrift service descriptors and
services from existing F# specifications.
