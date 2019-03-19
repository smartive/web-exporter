import './assets';
import './index.scss';

import React from 'react';
import ReactDOM from 'react-dom';
import ReactDOMServer from 'react-dom/server';

import * as Components from './components';

(global as any).React = React;
(global as any).ReactDOM = ReactDOM;
(global as any).ReactDOMServer = ReactDOMServer;

(global as any).Components = Components;
