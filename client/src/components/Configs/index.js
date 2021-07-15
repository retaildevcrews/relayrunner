import { useContext, useState } from 'react';
import ConfigsContext from '../ConfigsContext';

import "./styles.css"

const Configs = () => {
    const { configs } = useContext(ConfigsContext);

    const [currConfig, setCurrConfig] = useState(-1);
    console.log(currConfig)

    

    const configSelect = (id) => {
        if (currConfig === id) {
            setCurrConfig(-1);
        }
        else {
            setCurrConfig(id);
        }
    }

    return (
        <div className="main">
            <hr className="horizontal"></hr>
            <hr className="vertical"></hr>
            <div className="configs">
                <h1>Configs</h1>
                <hr></hr>
                <div>
                    <ul>
                    {
                        configs.map((c) => (
                            <li key={c.id}>
                                <button className={`configslist ${c.id === currConfig ? "selected" : ""}`} onClick={() => {configSelect(c.id)}}>{c.name}</button>
                            </li>
                        )
                        )
                    }
                    </ul>
                </div>
            </div>
        </div>

    )
}

export default Configs;