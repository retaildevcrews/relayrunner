import PropTypes from "prop-types";
import { SORT_TYPES, STATUSES } from "./constants";
import "./styles.css";

const ClientsFilter = ({ setFilterBy, setSortBy }) => {
  return (
    <>
      <select onChange={(e) => setSortBy(e.target.value)}>
        <option value={SORT_TYPES.name.key}>{SORT_TYPES.name.label}</option>
        <option value={SORT_TYPES.dateCreated.key}>
          {SORT_TYPES.dateCreated.label}
        </option>
      </select>
      <select onChange={(e) => setFilterBy(e.target.value)}>
        <option value="">Filter By:</option>
        {STATUSES.map(({ key, label }) => (
          <option key={key} value={key}>
            {label}
          </option>
        ))}
      </select>
    </>
  );
};

ClientsFilter.propTypes = {
  setFilterBy: PropTypes.func.isRequired,
  setSortBy: PropTypes.func.isRequired,
};

export default ClientsFilter;
