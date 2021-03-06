# Web Exporter for prometheus

![Build](https://gitlab.com/smartive/open-source/web-exporter/badges/master/pipeline.svg)
![Layers](https://img.shields.io/microbadger/layers/smartive/web-exporter.svg)
![Size](https://img.shields.io/microbadger/image-size/smartive/web-exporter.svg)

#### Demo

There is an online demo of this tool here: [Web-Exporter Demo](https://demo.web-exporter.smartive.cloud/)

This demo will be reset every four hours.

## Description

This is a "smart" exporter for prometheus. This exporter should extend / replace the
basic exporter of prometheus with the goal to perform more specific tests on api
calls. While you can scrap for web targets with the basic exporter, it is (merely) only
possible to check for a successful status code.

This exporter however comes with a small management UI that allows you to define the
tests by themself with url, http method and of course a name.

After this step, you may define additional labels that are exported to prometheus during
scrapping. You may also add request headers - like authorization headers, api version
headers, language headers and so forth.

Last but not least, you can define "response-tests" that can be written in javascript
in [`monaco`](https://microsoft.github.io/monaco-editor/) (which is the editor that powers vscode)
with `lodash` and `jsonpath` and a custom logger at your disposal. When some tests are defined,
the web-check will execute them with the request and the response one by one and the test will
only result in a `true` result when all of them are truthly executed.

The javascript code is run by a node `vm2` instance that has a very strict timeout of one
second to prevent malicious code like `while (true) {}` from running to infinity and beyond.

The metrics are exposed on the `/metrics` endpoint and can be crapped by prometheus at any time.

## Configuration

You can configure this exporter with the following environment variables:

| Variable            | Default value | Description |
| ------------------- | ------------- | ----------- |
| PROBE_INTERVAL      | 60            | Timer interval that kicks off execution of the checks |
| HTTP_TIMEOUT        | `null`        | When set, defines the timeout for http connections, defaults to `PROBE_INTERVAL` |
| AUTO_EXECUTE_CHECKS | true          | When true, automatic execution of checks is enabled, otherwise tests must be executed manually |
