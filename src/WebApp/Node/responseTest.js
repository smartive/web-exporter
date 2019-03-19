const _ = require('lodash');
const jsonpath = require('jsonpath');
const { VM } = require('vm2');

class Logger {
    constructor() {
        this.messages = [];
    }

    log(...messagesOrObjects) {
        this.writeMessage('log', messagesOrObjects);
    }

    info(...messagesOrObjects) {
        this.writeMessage('info', messagesOrObjects);
    }

    warn(...messagesOrObjects) {
        this.writeMessage('warn', messagesOrObjects);
    }

    error(...messagesOrObjects) {
        this.writeMessage('error', messagesOrObjects);
    }

    writeMessage(prefix, messages) {
        for (const message of messages) {
            this.messages.push(`[${prefix}]: ${typeof message === 'string' ? message : JSON.stringify(message)}`);
        }
    }
}

module.exports = function (callback, request, response, testFunction) {
    const logger = new Logger();
    const vm = new VM({
        timeout: 1000,
        sandbox: {
            _,
            jsonpath,
            logger,
            responseTest: func => func(request, response),
        },
    });

    try {
        const result = vm.run(testFunction);
        callback(null, {
            result,
            logs: logger.messages,
        });
    } catch (e) {
        callback(e, null);
    }
};
